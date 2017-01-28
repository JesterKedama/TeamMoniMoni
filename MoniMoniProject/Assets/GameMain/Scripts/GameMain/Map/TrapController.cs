using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// トラップの管理と操作
/// </summary>
public class TrapController : MonoBehaviour
{
    [SerializeField]
    EnemyManager enemymanager;

    [SerializeField]
    PlayerController player;

    [SerializeField]
    MapChipController mapchip;

    [SerializeField]
    EventTalkManager talkmanager;


    class TrapStatus
    {
        public Sprite sprite;
        public int trap_active_time;
    }
    /// <summary>
    ///トラップの名前、向き、画像の登録先
    /// </summary>
    Dictionary<string, Dictionary<int, TrapStatus>> trap_repository;
    // トラップフォルダの画像
    Sprite[] loadsprite;

    // マップにインスタンスされたトラップ
    List<GameObject> traps;

    // トラップ設置中かどうか
    bool is_puttrap;
    // トラップの向きを決める選択肢を出したかどうか
    bool is_select_trap_direction = false;

    void Start()
    {
        trap_repository = new Dictionary<string, Dictionary<int, TrapStatus>>();
        loadsprite = Resources.LoadAll<Sprite>("Textures/Trap");
        traps = new List<GameObject>();

        trapSetup("Easel", 80);
        trapSetup("Statue", 80);

        is_puttrap = false;

        StartCoroutine(trapCoroutine());
    }

    private IEnumerator trapCoroutine()
    {
        while (true)
        {
            if (is_puttrap)
            {
                player.state = PlayerController.State.TALK;
                yield return null;
                if (is_select_trap_direction == false)
                {
                    talkmanager.startTalk("put_trap");
                    is_select_trap_direction = true;
                }
                if (talkmanager.is_talknow == false)
                {
                    var direction = talkmanager.selectbuttonnum - 1;    //1から始まるので、1引く
                    var temptrap = trap_repository[player.have_item_name] [direction];
                    int x = mapchip.select_cell_x, y = mapchip.select_cell_y;

                    var trap = Resources.Load<GameObject>("Prefabs/Trap");
                    trap.GetComponent<SpriteRenderer>().sprite = temptrap.sprite;
                    trap.transform.position = mapchip.blocks[(int)LayerController.Layer.EVENT] [y] [x].transform.position + new Vector3(0,0,-0.1f);
                    trap.transform.localScale = new Vector2(mapchip.chip_scale, mapchip.chip_scale);

                    trap.GetComponent<Trap>().trap_active_time = temptrap.trap_active_time;
                    trap.GetComponent<Trap>().search_range = mapchip.chip_size;
                    trap.GetComponent<Trap>().direction = (Trap.TrapDirection)direction;

                    traps.Add((GameObject)Instantiate(trap, transform));

                    player.state = PlayerController.State.NORMAL;
                    is_puttrap = false;
                    is_select_trap_direction = false;
                    player.eventUseItem();
                }
            }
            yield return null;
        }

    }

    void Update()
    {

    }



    /// <summary>
    /// トラップを4方向登録する関数
    /// </summary>
    /// <param name="trapname_"></param>
    void trapSetup(string trapname_, int trap_active_time_)
    {
        var tempdic = new Dictionary<int, TrapStatus>();
        for (int i = 0; i < 4; i++)
        {
            TrapStatus temptrap = new TrapStatus();
            temptrap.sprite = System.Array.Find<Sprite>(
                                    loadsprite, (sprite) => sprite.name.Equals(
                                        trapname_ + "_" + i));
            temptrap.trap_active_time = trap_active_time_;
            tempdic.Add(i, temptrap);
        }
        trap_repository.Add(trapname_, tempdic);
    }

    /// <summary>
    /// アイテムボタンを押したらトラップを設置する関数
    /// </summary>
    public void putTrap()
    {
        // プレイヤーがアイテムを持っているかどうか
        if (player.isHaveItem() == false) return;
        // 持っているアイテムが設置できるトラップかどうか
        if (trap_repository.ContainsKey(player.have_item_name) == false) return;

        int x = mapchip.select_cell_x, y = mapchip.select_cell_y;
        // 設置しようとしているところに壁、ドア、物があるかどうか
        if (mapchip.isFloor(x, y) == false) return;

        is_puttrap = true;
    }

    // イベントでトラップを設置する関数
    public void eventPutTrap(string trap_name_, int trap_active_time_, int x, int y)
    {
        var trap = Resources.Load<GameObject>("Prefabs/Trap");
        trap.transform.position = mapchip.blocks[(int)LayerController.Layer.EVENT] [y] [x].transform.position + new Vector3(0, 0, -0.1f); ;
        trap.transform.localScale = new Vector2(mapchip.chip_scale, mapchip.chip_scale);

        trap.GetComponent<SpriteRenderer>().sprite = null;
        trap.GetComponent<Trap>().trap_active_time = trap_active_time_;
        trap.GetComponent<Trap>().search_range = mapchip.chip_size;

        traps.Add((GameObject)Instantiate(trap, transform));

        player.eventUseItem();
    }


}
