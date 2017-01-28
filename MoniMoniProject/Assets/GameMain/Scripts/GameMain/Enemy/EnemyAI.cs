using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using System.IO;

public class EnemyAI : MonoBehaviour
{

    // エネミーの番号
    public int enemynumber = 0;

    // エネミーが動けるかどうか
    public bool can_move;

    Animator anim;
    // エネミーの向き
    public enum EnemyDirection
    {
        UP, DOWN, RIGHT, LEFT, DIRECTION_MAX
    }

    public EnemyDirection direction = EnemyDirection.DOWN;
    // 今の向き
    public EnemyDirection current_direction = EnemyDirection.DOWN;
    // 前向いていた方向
    public EnemyDirection prev_direction = EnemyDirection.DOWN;

    public Vector2 vec;
    public float speed = 0;
    public float up_speed = 0;

    public int locateplayermode_count;
    public int locateplayermode_maxcount;

    public enum State
    {
        IDLE,                       // 移動しない状態
        ROOT_NORMALMOVE,            // 移動している状態
        ROOT_LOCATEPLAYERBACKMOVE,  // 前方にプレイヤーを見つけて後ろに逃げる関数
        ROOT_CHANGE,                // 通っていたルートを変更してる状態
        TRAP,                       // トラップにかかった状態
    }

    public State state;
    State currentstate;

    // 警戒モードかどうか
    bool is_speedupmode;

    public MapChipController mapchip;
    public PlayerController player;

    public enum RootType
    {
        NONE,   // 道なし
        START,  // 道の始まり
        ROOT,   // 道
    }

    // ルートの配列(ルートの種類,y,x)
    public List<List<List<RootType>>> roots;
    public RootType current_root;

    // ルートの数
    public int rootmax;
    // 通っているルート番号
    public int rootnum;
    public int current_rootnum;

    // 周回数
    public int root_roundcount;
    // 周回数によってルートを変更する回数
    public int root_roundmax;

    // プレイヤーを探す矩形   structだと値の代入ができなかった、なぜ？
    class PlayerSearchRect
    {
        public Vector2 pos;
        // typeの仕様
        // 0    なし
        // 1    エネミーの位置
        // 2    プレイヤーを調べるところ
        public int type;
    }
    // 向き、y,x
    List<List<List<PlayerSearchRect>>> front_search_rects;
    List<List<List<PlayerSearchRect>>> search_rects;

    Dictionary<EnemyDirection, Vector2> search_rect_enemypos;
    // 探す矩形の大きさ
    Vector2 search_range;


    void Start()
    {
        aiSetup();
    }

    public void aiSetup()
    {
        // プレハブからヒエラルキーのオブジェクトがもらえなかったので仕方なく名前検索
        mapchip = GameObject.Find("MapManager").GetComponent<MapChipController>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();

        vec = new Vector2();
        currentstate = State.IDLE;
        state = State.ROOT_CHANGE;
        anim = GetComponent<Animator>();
        roots = new List<List<List<RootType>>>();
        rootsLoad();
        rootRandomDecide();
        playerSearchRectSetup();

        is_speedupmode = false;
    }


    void Update()
    {
        if (can_move == false) return;
        stateChangeUpdate();
        animationUpdate();
        rootmove();
        rootChangeMove();
        playerSearch();
        locatePlayerModeUpdate();
        trapUpdate();
    }

    void animationUpdate()
    {
        anim.SetInteger("enemy_direction", (int)direction);
        anim.SetFloat("up_down_vec", vec.y * 100);
        anim.SetFloat("right_left_vec", vec.x * 100);
    }

    // 移動先のマップ番号
    Vector2 nextmovecell;

    // 移動した量
    float move_value;

    /// <summary>
    /// ルートを移動する関数
    /// </summary>
    private void rootmove()
    {
        if (state == State.ROOT_NORMALMOVE ||
            state == State.ROOT_LOCATEPLAYERBACKMOVE)
        {
            if (move_value >= mapchip.chip_size)
            {
                move_value = 0;
                int x = (int)retCell().x;
                int y = (int)retCell().y;
                adjustPosition(x, y);

                nextmovecell = nextMoveCell();
                if (roots[rootnum][y][x] == RootType.START)
                {
                    root_roundcount++;
                    if (root_roundcount >= root_roundmax)
                    {
                        state = State.ROOT_CHANGE;
                        root_roundcount = 0;
                        return;
                    }
                }
                directionChange(x, y, (int)nextmovecell.x, (int)nextmovecell.y);
                directionSave();
            }
            else
            {
                float s;
                if (is_speedupmode)
                    s = up_speed;
                else
                    s = speed;
                directionMove(s);
                move_value += s;
            }
        }

    }

    /// <summary>
    /// 向きによって移動する関数
    /// </summary>
    void directionMove(float speed_)
    {
        vec = Vector2.zero;

        switch (direction)
        {
            case EnemyDirection.UP:
                vec.y += speed_;
                break;
            case EnemyDirection.DOWN:
                vec.y -= speed_;
                break;
            case EnemyDirection.RIGHT:
                vec.x += speed_;
                break;
            case EnemyDirection.LEFT:
                vec.x -= speed_;
                break;
        }
        transform.Translate(vec);
    }

    /// <summary>
    /// 今乗っているブロックと今向いている方向から次移動する番号を返す関数
    /// </summary>
    Vector2 nextMoveCell()
    {
        int cell_x = (int)retCell().x;
        int cell_y = (int)retCell().y;
        cell_x = Mathf.Clamp(cell_x, 0, mapchip.chip_num_x - 1);
        cell_y = Mathf.Clamp(cell_y, 0, mapchip.chip_num_y - 1);

        int next_cell_x = cell_x;
        int next_cell_y = cell_y;

        switch (direction)
        {
            case EnemyDirection.UP:
                // 前
                if (cell_y - 1 > 0)
                    if (roots[rootnum][cell_y - 1][cell_x] != RootType.NONE)
                    {
                        next_cell_y -= 1;
                        break;
                    }
                // 左
                if (cell_x - 1 > 0)
                    if (roots[rootnum][cell_y][cell_x - 1] != RootType.NONE)
                    {
                        next_cell_x -= 1;
                        break;
                    }
                // 右
                if (cell_x + 1 < mapchip.chip_num_x)
                    if (roots[rootnum][cell_y][cell_x + 1] != RootType.NONE)
                    {
                        next_cell_x += 1;
                        break;
                    }
                // 後ろ
                next_cell_y += 1;
                break;
            case EnemyDirection.DOWN:
                // 前
                if (cell_y + 1 < mapchip.chip_num_y)
                    if (roots[rootnum][cell_y + 1][cell_x] != RootType.NONE)
                    {
                        next_cell_y += 1;
                        break;
                    }
                // 左
                if (cell_x + 1 < mapchip.chip_num_x)
                    if (roots[rootnum][cell_y][cell_x + 1] != RootType.NONE)
                    {
                        next_cell_x += 1;
                        break;
                    }
                // 右
                if (cell_x - 1 > 0)
                    if (roots[rootnum][cell_y][cell_x - 1] != RootType.NONE)
                    {
                        next_cell_x -= 1;
                        break;
                    }
                // 後ろ
                next_cell_y -= 1;
                break;
            case EnemyDirection.RIGHT:
                // 前
                if (cell_x + 1 < mapchip.chip_num_x)
                    if (roots[rootnum][cell_y][cell_x + 1] != RootType.NONE)
                    {
                        next_cell_x += 1;
                        break;
                    }
                // 左
                if (cell_y - 1 > 0)
                    if (roots[rootnum][cell_y - 1][cell_x] != RootType.NONE)
                    {
                        next_cell_y -= 1;
                        break;
                    }
                // 右
                if (cell_y + 1 < mapchip.chip_num_y)
                    if (roots[rootnum][cell_y + 1][cell_x] != RootType.NONE)
                    {
                        next_cell_y += 1;
                        break;
                    }
                // 後ろ
                next_cell_x -= 1;
                break;
            case EnemyDirection.LEFT:
                // 前
                if (cell_x - 1 > 0)
                    if (roots[rootnum][cell_y][cell_x - 1] != RootType.NONE)
                    {
                        next_cell_x -= 1;
                        break;
                    }
                // 左
                if (cell_y + 1 < mapchip.chip_num_y)
                    if (roots[rootnum][cell_y + 1][cell_x] != RootType.NONE)
                    {
                        next_cell_y += 1;
                        break;
                    }
                // 右
                if (cell_y - 1 > 0)
                    if (roots[rootnum][cell_y - 1][cell_x] != RootType.NONE)
                    {
                        next_cell_y -= 1;
                        break;
                    }

                // 後ろ
                next_cell_x += 1;
                break;
        }

        return new Vector2(next_cell_x, next_cell_y);
    }

    /// <summary>
    /// 今の番号と移動先の番号で方向を変える関数
    /// </summary>
    /// <param name="current_">今の番号x,y</param>
    /// <param name="next_">移動先の番号x,y</param>
    void directionChange(int current_x_, int current_y_, int next_x_, int next_y_)
    {
        if (current_y_ > next_y_)
        {
            direction = EnemyDirection.UP;
            return;
        }
        else if (current_y_ < next_y_)
        {
            direction = EnemyDirection.DOWN;
            return;
        }
        if (current_x_ < next_x_)
        {
            direction = EnemyDirection.RIGHT;
            return;
        }
        else if (current_x_ > next_x_)
        {
            direction = EnemyDirection.LEFT;
        }
    }

    /// <summary>
    /// 前の方向を保存しておく
    /// </summary>
    void directionSave()
    {
        if (direction != current_direction)
        {
            prev_direction = current_direction;
            current_direction = direction;
        }
    }

    void directionBackChange()
    {
        direction = prev_direction;
        //switch (direction)
        //{
        //    case EnemyDirection.UP:
        //        direction = EnemyDirection.DOWN;
        //        break;
        //    case EnemyDirection.DOWN:
        //        direction = EnemyDirection.UP;
        //        break;
        //    case EnemyDirection.RIGHT:
        //        direction = EnemyDirection.LEFT;
        //        break;
        //    case EnemyDirection.LEFT:
        //        direction = EnemyDirection.RIGHT;
        //        break;
        //}
    }

    /// <summary>
    /// もらった番号のブロックの中心位置に自分を移動させる
    /// </summary>
    private void adjustPosition(int x_, int y_)
    {
        transform.position = new Vector3(mapchip.blocks[(int)LayerController.Layer.FLOOR][y_][x_].transform.position.x,
            mapchip.blocks[(int)LayerController.Layer.FLOOR][y_][x_].transform.position.y,
            -0.4f);
    }

    /// <summary>
    /// enemyの位置からcell番号(x, y)を返す
    /// </summary>
    /// <returns>cell番号</returns>
    public Vector2 retCell()
    {
        var pos = transform.position;
        Vector2 cell_f = pos / mapchip.chip_size;

        Vector2 cell_i = new Vector2(Mathf.RoundToInt(cell_f.x), Mathf.RoundToInt(cell_f.y));
        cell_i.x = Mathf.Abs(cell_i.x);
        cell_i.y = Mathf.Abs(cell_i.y);

        return cell_i;
    }

    // A*

    enum AstarState
    {
        SEARCH,
        WORK_START,
        WORK,
        END,
    }

    AstarState astarstate;
    AStar.RootPosition goalpos;
    AStar.NodeManager nodemanager;
    AStar.Node node;
    List<AStar.RootPosition> rootlist;

    int astar_trycount = 0;
    int astarmovecount = 0;

    /// <summary>
    /// A*準備
    /// </summary>
    void astarSetup()
    {
        rootRandomDecide();
        astarstate = AstarState.SEARCH;

        int x = (int)retCell().x;
        int y = (int)retCell().y;
        var startpos = new AStar.RootPosition(x, y);

        Vector2 tempgoalpos;
        tempgoalpos = getGoalCell();
        goalpos = new AStar.RootPosition((int)tempgoalpos.x, (int)tempgoalpos.y);

        nodemanager = new AStar.NodeManager(goalpos.x, goalpos.y, mapchip);
        node = nodemanager.openNode(startpos.x, startpos.y, 0, null);
        nodemanager.addOpenNode(node);

        rootlist = new List<AStar.RootPosition>();

        astar_trycount = 0;
        astarmovecount = 0;
    }

    /// <summary>
    /// A*して次のルートの開始位置に移動する関数
    /// </summary>
    void rootChangeMove()
    {
        if (state != State.ROOT_CHANGE) return;
        int x = (int)retCell().x;
        int y = (int)retCell().y;
        if (astarstate == AstarState.SEARCH)
        {
            while (astar_trycount < 5000)
            {
                nodemanager.removeOpenNode(node);
                nodemanager.openAround(node);

                node = nodemanager.searchMinScoreNodeFromOpenNodeList();

                if (node == null)
                    break;
                if (node.x == goalpos.x && node.y == goalpos.y)
                {
                    nodemanager.removeOpenNode(node);
                    node.getRootList(rootlist);
                    rootlist.Reverse();
                    astarstate = AstarState.WORK_START;
                    astar_trycount = 0;
                    break;
                }
                astar_trycount++;
            }
        }
        else if (astarstate == AstarState.WORK_START)
        {
            if (astarmovecount >= rootlist.Count - 1)
            {
                astarmovecount = 0;
                state = State.ROOT_NORMALMOVE;
                return;
            }
            astarmovecount++;
            nextmovecell = new Vector2(rootlist[astarmovecount].x, rootlist[astarmovecount].y);
            directionChange(x, y, (int)nextmovecell.x, (int)nextmovecell.y);
            astarstate = AstarState.WORK;
        }
        else if (astarstate == AstarState.WORK)
        {
            float s;
            if (is_speedupmode)
                s = up_speed;
            else
                s = speed;
            directionMove(s);
            move_value += s;
            if (move_value >= mapchip.chip_size)
            {
                adjustPosition(x, y);
                astarstate = AstarState.WORK_START;
                move_value = 0;
            }
        }
    }

    /// <summary>
    /// ルートをランダムに変更する関数（同じルートは通らない）
    /// </summary>
    void rootRandomDecide()
    {
        if (rootmax <= 0) return;
        while (true)
        {
            int rand_num = Random.Range(0, rootmax);
            if (rand_num != rootnum)
            {
                rootnum = rand_num;
                break;
            }
        }
    }

    Vector2 getGoalCell()
    {
        Vector2 goalpos = new Vector2();

        for (int y = 0; y < mapchip.chip_num_y; y++)
        {
            for (int x = 0; x < mapchip.chip_num_x; x++)
            {
                if (roots[rootnum][y][x] == RootType.START)
                    goalpos = new Vector2(x, y);
            }
        }

        return goalpos;
    }

    /// <summary>
    /// エネミーの状態が変わった時に何かを起こす関数
    /// </summary>
    void stateChangeUpdate()
    {
        if (currentstate == state) return;
        switch (state)
        {
            case State.IDLE:
                break;
            case State.ROOT_NORMALMOVE:
                nextmovecell = nextMoveCell();
                if (currentstate == State.ROOT_LOCATEPLAYERBACKMOVE)
                    directionBackChange();
                break;
            case State.ROOT_LOCATEPLAYERBACKMOVE:
                nextmovecell = nextMoveCell();
                if (currentstate == State.ROOT_NORMALMOVE)
                    directionBackChange();
                break;
            case State.ROOT_CHANGE:
                astarSetup();
                break;
            case State.TRAP:
                trapSetup();
                break;
        }
        currentstate = state;
    }

    /// <summary>
    /// ルート読み込み
    /// </summary>
    void rootsLoad()
    {
        int chip_x = mapchip.chip_num_x;
        int chip_y = mapchip.chip_num_y;

        rootmax = 0;
        while (true)
        {
            var readtext = Resources.Load<TextAsset>("StageData/"
               + mapchip.select_map_name
               + "_Enemy" + enemynumber
               + "Root" + rootmax + "Data");
            // テキストが見つからなかったら抜ける
            if (readtext == null)
                break;

            int[][] loadrootnums = new int[chip_y][];
            for (int y = 0; y < chip_y; y++)
            {
                loadrootnums[y] = new int[chip_x];
            }

            using (var sr = new StringReader(readtext.text))
            {
                // スタートの番号
                int usestartnum = int.MaxValue;

                for (int y = 0; y < chip_y; y++)
                {
                    string line = sr.ReadLine();
                    for (int x = 0; x < chip_x; x++)
                    {
                        loadrootnums[y][x] = mapchip.stringToInt(line, x);
                        if (loadrootnums[y][x] != -1)
                        {
                            if (usestartnum > loadrootnums[y][x])
                                usestartnum = loadrootnums[y][x];
                        }
                    }
                }

                List<List<RootType>> temproot_xy = new List<List<RootType>>();
                for (int y = 0; y < chip_y; y++)
                {
                    List<RootType> temploot_x = new List<RootType>();
                    for (int x = 0; x < chip_x; x++)
                    {
                        RootType tempnum = RootType.NONE;
                        if (loadrootnums[y][x] != -1)
                        {
                            if (usestartnum == loadrootnums[y][x])
                            {
                                tempnum = RootType.START;
                            }
                            else
                            {
                                tempnum = RootType.ROOT;
                            }
                        }

                        temploot_x.Add(tempnum);
                    }
                    temproot_xy.Add(temploot_x);
                }
                roots.Add(temproot_xy);
                sr.Close();
            }
            rootmax++;
        }
    }

    /// <summary>
    /// プレイヤーを探索する範囲決め
    /// </summary>
    void playerSearchRectSetup()
    {
        search_range = new Vector2(mapchip.chip_size, mapchip.chip_size);
        front_search_rects = new List<List<List<PlayerSearchRect>>>();
        search_rects = new List<List<List<PlayerSearchRect>>>();
        search_rect_enemypos = new Dictionary<EnemyDirection, Vector2>();

        for (int i = 0; i < (int)EnemyDirection.DIRECTION_MAX; i++)
        {
            front_search_rects.Add(createSearchRect("EnemyAI/EnemyFrontSearchRange" + i.ToString(), (EnemyDirection)i));
            search_rects.Add(createSearchRect("EnemyAI/EnemySearchRange" + i.ToString(), (EnemyDirection)i));
        }
    }

    List<List<PlayerSearchRect>> createSearchRect(string resources_readtext_, EnemyDirection direction)
    {
        var readtext = Resources.Load<TextAsset>(resources_readtext_);
        using (var sr = new StringReader(readtext.text))
        {
            var tempsr = new StringReader(readtext.text);
            int element_count = mapchip.stringToSpaceBoundLength(tempsr.ReadLine());

            List<List<PlayerSearchRect>> temprect_xy = new List<List<PlayerSearchRect>>();
            for (int y = 0; y < element_count; y++)
            {
                string line = sr.ReadLine();
                List<PlayerSearchRect> temprect_x = new List<PlayerSearchRect>();
                for (int x = 0; x < element_count; x++)
                {
                    PlayerSearchRect rect = new PlayerSearchRect();
                    rect.pos = new Vector2(search_range.x * x, search_range.y * y);
                    rect.type = mapchip.stringToInt(line, x);
                    if (rect.type == 1)
                    {
                        if (search_rect_enemypos.ContainsKey(direction) == false)
                            search_rect_enemypos.Add(direction, rect.pos);
                    }
                    temprect_x.Add(rect);
                }
                temprect_xy.Add(temprect_x);
            }
            return temprect_xy;
        }
    }

    /// <summary>
    /// プレイヤーを探して、いろいろする関数(あんまりきれいじゃない；；)
    /// </summary>
    void playerSearch()
    {
        int d = (int)direction;
        for (int y = 0; y < search_rects[d].Count; y++)
        {
            for (int x = 0; x < search_rects[d][y].Count; x++)
            {
                var search_pos = search_rects[d][y][x].pos +
                    new Vector2(transform.position.x, transform.position.y) -
                    search_rect_enemypos[direction];
                if (search_rects[d][y][x].type == 2 &&
                    mapchip.pointToCenterBoxRect(player.transform.position,
                    search_pos, search_range))
                {
                    is_speedupmode = true;
                }
                if (front_search_rects[d][y][x].type == 2 &&
                mapchip.pointToCenterBoxRect(player.transform.position,
                search_pos, search_range))
                {
                    if (state == State.ROOT_NORMALMOVE)
                        state = State.ROOT_LOCATEPLAYERBACKMOVE;
                    else if (state == State.ROOT_LOCATEPLAYERBACKMOVE)
                        state = State.ROOT_NORMALMOVE;
                    // 見つけてない状態で前方方向にプレイヤーを見つけた場合
                    // ルートを変更する
                    if (is_speedupmode == false &&
                        state == State.ROOT_CHANGE)
                    {
                        astarSetup();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 警戒モードのアップデート
    /// </summary>
    void locatePlayerModeUpdate()
    {
        if (is_speedupmode) return;
        locateplayermode_count++;
        if (locateplayermode_count >= locateplayermode_maxcount)
        {
            is_speedupmode = false;
            locateplayermode_count = 0;
        }
    }

    // 罠

    /// <summary>
    /// トラップを調べる
    /// </summary>
    void trapSearch()
    {
        switch (direction)
        {
            case EnemyDirection.UP:

                break;
            case EnemyDirection.DOWN:
                break;
            case EnemyDirection.RIGHT:
                break;
            case EnemyDirection.LEFT:
                break;
        }
    }

    Vector3 trapstaging_start_pos = new Vector3();
    int trap_active_time;
    int trap_count;

    public void inTrap(int trap_active_time_)
    {
        state = State.TRAP;
        trap_active_time = trap_active_time_;
    }

    void trapSetup()
    {
        trapstaging_start_pos = transform.position;
    }

    void trapUpdate()
    {
        if (state != State.TRAP) return;
        trap_count++;
        transform.position = trapstaging_start_pos;
        Vector3 randompos = Random.insideUnitCircle * 0.02f;
        transform.Translate(randompos);

        if (trap_count > trap_active_time)
        {
            state = State.ROOT_CHANGE;
            trap_count = 0;
        }
    }
}
