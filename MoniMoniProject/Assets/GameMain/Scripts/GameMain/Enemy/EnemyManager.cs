using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    NightMareController nightmare;
    [SerializeField]
    EnemyAI nightmareAI;

    [SerializeField]
    PlayerController player;

    [SerializeField]
    MapChipController mapchip;

    public List<GameObject> enemys;

    // エネミーの数
    public int enemy_num;

    // 捕まえる画面に移行するためのbool
    public bool is_bosshit;
    public bool is_enemy_hit;

    void Start()
    {
        enemySetup();
    }

    /// <summary>
    /// エネミーの準備
    /// </summary>
    public void enemySetup()
    {

        if (mapchip.select_map_name.IndexOf("House") != -1)
        {
            nightmare.gameObject.SetActive(false);
            dreamersPop(0);
        }
        else
        {
            nightmare.gameObject.SetActive(true);

            if (mapchip.select_stage_name == "School")
                enemy_num = SceneInfoManager.instance.school_enemy_num;
            else if (mapchip.select_stage_name == "Hospital")
                enemy_num = SceneInfoManager.instance.hospital_enemy_num;

            dreamersPop(enemy_num);
            nightmareAI.aiSetup();
        }
        is_bosshit = false;

        for (int y = 0; y < mapchip.chip_num_y; y++)
        {
            for (int x = 0; x < mapchip.chip_num_x; x++)
            {
                if (mapchip.blockcomponents[(int)LayerController.Layer.EVENT][y][x].number == 22)
                {
                    nightmare.transform.position =
                    new Vector3(mapchip.blocks[(int)LayerController.Layer.FLOOR][y][x].transform.position.x,
                    mapchip.blocks[(int)LayerController.Layer.FLOOR][y][x].transform.position.y,
                    nightmare.transform.position.z);
                    break;
                }

            }
        }
    }

    /// <summary>
    /// ナイトメアを動く状態にする関数
    /// </summary>
    /// <param name="selectnum_"></param>
    public void nightmareRePop(int selectnum_)
    {
        if (selectnum_ == 2)
        {
            nightmare.animationChange();
            nightmareAI.state = EnemyAI.State.ROOT_CHANGE;
            nightmareAI.can_move = true;
            for (int i = 0; i < enemy_num; i++)
            {
                Destroy(enemys[i]);
            }
            enemys.Clear();
            nightmare.is_move = true;
            nightmare.prev_cell = nightmare.retCell();
            enemy_num = 0;
        }
    }

    void Update()
    {
        if (nightmare.is_move == false)
            nightmareAI.state = EnemyAI.State.IDLE;
        if (nightmare.can_capture)
            if (pointToCircle(player.transform.position, 0.3f, nightmare.transform.position))
            {
                is_bosshit = true;
            }

        if (mapchip.select_map_name.IndexOf("House") != -1) return;
        for (int i = 0; i < enemy_num; i++)
        {
            if (pointToCircle(player.transform.position, 0.3f, enemys[i].transform.position))
            {
                is_enemy_hit = true;
            }
        }

        if (player.state == PlayerController.State.EVENT ||
            player.state == PlayerController.State.TALK)
        {
            for (int i = 0; i < enemy_num; i++)
            {
                enemys[i].GetComponent<EnemyAI>().can_move = false;
            }
        }
        else
        {
            for (int i = 0; i < enemy_num; i++)
            {
                enemys[i].GetComponent<EnemyAI>().can_move = true;
            }
        }
    }

    /// <summary>
    /// 敵を出現させる関数
    /// </summary>
    /// <param name="enemy_num_"></param>
    void dreamersPop(int enemy_num_)
    {
        for (int i = 0; i < enemy_num_; i++)
        {
            var enemy = Resources.Load<GameObject>("Prefabs/Enemy");
            var ai = enemy.GetComponent<EnemyAI>();
            ai.mapchip = mapchip;
            ai.direction = EnemyAI.EnemyDirection.DOWN;
            ai.enemynumber = i;

            enemy.transform.position = new Vector3(4.0f, -58.0f, -0.4f);

            enemy.transform.localScale = new Vector3(0.5f, 0.5f, 0.0f);

            enemys.Add((GameObject)Instantiate(enemy, transform));
        }
    }

    /// <summary>
    /// 点と円の判定(当たってたらtrue)
    /// </summary>
    bool pointToCircle(Vector2 circlepos, float radius, Vector2 pointpos)
    {
        float x = (pointpos.x - circlepos.x) * (pointpos.x - circlepos.x);
        float y = (pointpos.y - circlepos.y) * (pointpos.y - circlepos.y);

        return x + y <= radius * radius;
    }

    /// <summary>
    /// エネミーにトラップにかかったことを教える関数
    /// </summary>
    public void enemyInTrap(int enemy_num_, int trap_active_time_)
    {
        enemys[enemy_num_].GetComponent<EnemyAI>().inTrap(trap_active_time_);
    }
}
