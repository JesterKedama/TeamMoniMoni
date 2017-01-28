using UnityEngine;
using System.Collections;

/// <summary>
/// シーン間の情報を保存する場所
/// MonoBehaviourを継承していないのでunityの機能が使えないことに注意
/// </summary>
public class SceneInfoManager
{
    public readonly static SceneInfoManager instance = new SceneInfoManager();

    // チュートリアルかどうか
    public bool is_tutorial = true;
    // チュートリアルが終わっているかどうか
    public bool is_tutorial_end = false;

    // シーンを移る際中かどうか
    public bool is_scenechange = false;

    // 今いたステージの名前(Schoolなど)
    public string select_stage_name = null;
    // 今いたマップの名前(House1Fなど)
    public string select_map_name = null;

    // プレイヤーの位置
    public Vector3 player_pos = new Vector3();
    // 持っているアイテムの名前
    public string have_item_name = null;

    // エネミーを倒した数
    public int enemy_kill_count = 0;

    // 残っているエネミーの数
    public int school_enemy_num = 3;
    public int hospital_enemy_num = 3;

    // ステージをクリアしているかどうか
    public bool is_shoolclear = false;
    public bool is_hospitalclear = false;

    public enum EndingStatus
    {
        NOT_END,
        GOOD_END,
        HUNGRY_END,
    }

    public EndingStatus endingstatus = EndingStatus.NOT_END;
}
