using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// イベントたちを作るクラス
/// イベントの作り方
/// 各関数ごとにintの返り値がある
/// return 0    イベント続行中
///        1    イベントを終了させるかつ、呼び出されるたびに同じイベントを繰り返す
///        2    イベントを終了させるかつ、イベントの段階を進める
/// </summary>
public class EventsCreate : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    [SerializeField]
    PlayerController playercontroller;

    [SerializeField]
    GameObject mapcontroller;

    [SerializeField]
    MapChipController chipcontroller;

    [SerializeField]
    EventTalkManager talkmanager;
    [SerializeField]
    EventLoader eventloader;
    [SerializeField]
    EventRepository eventrepository;

    [SerializeField]
    StagingController staging;

    [SerializeField]
    GameObject stagemoveeventcanvas;

    [SerializeField]
    EnemyManager enemymanager;
    [SerializeField]
    TrapController trapcontroller;

    [SerializeField]
    GameObject viba_button;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  シーン切り替えするときに呼ばれる関数                                                                                               //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // ドリーマーを捕まえるときに状態を保存する関数
    public void stateSave()
    {
        SoundManager.Instance.StopBGM();
        SceneInfoManager.instance.player_pos = player.transform.position;
        SceneInfoManager.instance.have_item_name = playercontroller.have_item_name;

        SceneInfoManager.instance.select_map_name = chipcontroller.select_map_name;
        SceneInfoManager.instance.select_stage_name = chipcontroller.select_stage_name;
        if (chipcontroller.select_stage_name == "School")
        {
            SceneInfoManager.instance.school_enemy_num = enemymanager.enemy_num;
            if (SceneInfoManager.instance.school_enemy_num > 0)
                SceneInfoManager.instance.school_enemy_num--;
        }
        else if (chipcontroller.select_stage_name == "Hospital")
        {
            SceneInfoManager.instance.hospital_enemy_num = enemymanager.enemy_num;
            if (SceneInfoManager.instance.hospital_enemy_num > 0)
                SceneInfoManager.instance.hospital_enemy_num--;
        }

    }

    // 学校のボス倒して帰ってくる関数
    public void schoolEndReturnHouse()
    {
        SoundManager.Instance.StopBGM();
        SceneInfoManager.instance.player_pos = new Vector3();
        SceneInfoManager.instance.select_map_name = "House1F";
        SceneInfoManager.instance.select_stage_name = "Videl";
        SceneInfoManager.instance.is_shoolclear = true;

        //// ここをのちに病院の条件も追加する
        //if (SceneInfoManager.instance.school_enemy_num <= 0)
        ////&&SceneInfoManager.instance.hospital_enemy_num <= 0) 
        //{
        //    SceneInfoManager.instance.endingstatus = SceneInfoManager.EndingStatus.GOOD_END;
        //}
        //else
        //{
        //    SceneInfoManager.instance.endingstatus = SceneInfoManager.EndingStatus.HUNGRY_END;
        //}
    }

    public void tutorialEndScenarioChange()
    {
        SoundManager.Instance.StopBGM();
        SceneInfoManager.instance.select_map_name = "House1FCenter";
        SceneInfoManager.instance.select_stage_name = "Videl";
        SceneInfoManager.instance.is_tutorial = false;
        SceneInfoManager.instance.is_tutorial_end = true;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  セットアップ                                                                                                                      //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void tutorialReturnSetup()
    {
        if (SceneInfoManager.instance.is_tutorial_end)
        {
            tutorialReturnToRoom();
            SceneInfoManager.instance.is_tutorial_end = false;
        }
        if (SceneInfoManager.instance.is_tutorial == false)
            viba_button.SetActive(true);

    }
    void Start()
    {
        SceneInfoManager.instance.is_scenechange = false;
        tutorialReturnSetup();
        schoolBlackOutEventSetup();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  アップデート                                                                                                                      //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void Update()
    {
        bgmUpdate();
        roomMoveUpdate();
        schoolBlackOutEvent();
    }

    void bgmUpdate()
    {

        if (chipcontroller.select_stage_name == "Videl")
        {
            SoundManager.Instance.volume.BGM = 0.2f;
            SoundManager.Instance.PlayBGM(0);
        }
        if (chipcontroller.select_stage_name == "School")
        {
            SoundManager.Instance.volume.BGM = 0.2f;
            SoundManager.Instance.PlayBGM(1);
        }
        if (SceneInfoManager.instance.is_scenechange)
        {
            SoundManager.Instance.StopBGM();
        }
    }

    int upcount = 0;
    // イベントが正常に動くかどうかのお試し関数２つ
    public int playerUpEvent()
    {
        player.transform.Translate(0, chipcontroller.chip_size, 0);
        upcount++;
        if (upcount < 1)
            return 1;
        return 2;
    }
    public int playerDirectionEvent()
    {
        if (playercontroller.player_direction == PlayerController.PlayerDirection.UP)
        {
            player.transform.Translate(0, 1.5f, 0);
        }
        if (playercontroller.player_direction == PlayerController.PlayerDirection.DOWN)
        {
            player.transform.Translate(0, -1.5f, 0);
        }
        upcount++;
        if (upcount < 1)
            return 1;
        return 1;
    }

    // 実装できてないイベントに入れる
    public int emptyEvent()
    {
        return 2;
    }
    public int overrapEmptyEvent()
    {
        playercontroller.state = PlayerController.State.NORMAL;
        return 1;
    }

    // 手が出てくるイベントを作ったけど、使ってないs
    float player_z = -0.5f;
    int zombiecount = 0;
    Vector2 player_start_pos;
    public int playerZombieHand2()
    {
        if (is_setup)
        {
            talkmanager.startTalk("testevent");
            is_setup = false;
        }

        if (talkmanager.is_talknow)
            return 0;
        zombiecount = 0;
        return 1;
    }


    bool is_setup = false;

    /// <summary>
    /// 会話だけの時に使用する関数
    /// </summary>
    /// <returns></returns>
    int talkEvent(string talkname_)
    {
        if (is_setup == false)
        {
            talkmanager.startTalk(talkname_);
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }

    /// <summary>
    /// 会話だけの時かつ、次のイベントに進む関数
    /// </summary>
    /// <param name="talkname_"></param>
    /// <returns></returns>
    int talkNextEvent(string talkname_)
    {
        if (is_setup == false)
        {
            talkmanager.startTalk(talkname_);
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 2;
    }


    /// イベントの作り方
    /// 各関数ごとにintの返り値がある
    /// return 0    イベント続行中
    ///        1    イベントを終了させるかつ、呼び出されるたびに同じイベントを繰り返す
    ///        2    イベントを終了させるかつ、イベントの段階を進める
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  学校イベント                                                                                                                      //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public bool is_schoolbossend = false;

    // 停電イベントが起きたかどうか
    bool is_blackoutevent = false;
    bool is_blackout;
    int blackoutstartcount;
    int blackoutcount;

    [SerializeField]
    Image blackout_background;

    void schoolBlackOutEventSetup()
    {
        blackoutstartcount = 4 * 60 * 60;
        blackoutcount = 0;
        is_blackout = false;
    }
    void schoolBlackOutEvent()
    {
        if (is_blackoutevent) return;

        if (chipcontroller.select_stage_name == "School" &&
            playercontroller.state == PlayerController.State.NORMAL)
        {
            blackoutcount++;
            if (blackoutcount > blackoutstartcount &&
                is_setup == false)
            {
                talkmanager.startTalk("School/school_10");

                stagemoveeventcanvas.SetActive(true);
                blackout_background.gameObject.SetActive(true);
                blackout_background.color = new Color(0, 0, 0);

                is_blackout = true;
                is_setup = true;
            }
        }
        if (is_blackout)
        {
            if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                talkmanager.event_call_count == 1)
            {
                SoundManager.Instance.volume.SE = 0.5f;
                SoundManager.Instance.PlaySE(0);
                talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
                return;
            }
            if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                talkmanager.event_call_count == 2)
            {
                SoundManager.Instance.PlaySE(0);

                blackout_background.color = new Color(255, 255, 255);
                stagemoveeventcanvas.SetActive(false);
                blackout_background.gameObject.SetActive(false);

                talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
            }
        }
        else
            return;
        playercontroller.state = PlayerController.State.TALK;
        if (talkmanager.is_talknow)
            return;
        playercontroller.state = PlayerController.State.NORMAL;
        is_setup = false;
        is_blackoutevent = true;
    }


    bool is_flush = false;
    public int schoolEvent01()
    {
        if (is_flush == false)
        {
            if (is_setup == false)
            {
                stagemoveeventcanvas.SetActive(true);
                talkmanager.startTalk("School/school_scenario");
                is_setup = true;
            }
            playercontroller.state = PlayerController.State.TALK;
        }
        if (talkmanager.is_talknow)
            return 0;
        if (is_flush == false)
        {
            if (staging.flushStart())
            {
                is_flush = true;
                is_setup = false;
                stagemoveeventcanvas.SetActive(false);
            }
            return 0;
        }
        else
        {
            if (staging.flushEnd())
            {
                return 2;
            }
            return 0;
        }
    }

    public int schoolEvent01_2()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_01");
            is_setup = true;
        }
        playercontroller.state = PlayerController.State.TALK;
        if (talkmanager.is_talknow)
            return 0;
        playercontroller.state = PlayerController.State.NORMAL;
        is_setup = false;
        return 2;
    }

    public int schoolEvent02()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_02");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        if (talkmanager.selectbuttonnum == 1)
            playercontroller.setItem("Boarderaser");
        is_setup = false;
        return 1;
    }
    public int schoolEvent03()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_03");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        if (talkmanager.selectbuttonnum == 1)
        {
            if (playercontroller.have_item_name == "Boarderaser")
            {
                trapcontroller.eventPutTrap("Boarderaser", 80, chipcontroller.eventselect_cell_x, chipcontroller.eventselect_cell_y);
            }
        }
        is_setup = false;
        return 1;
    }
    public int schoolEvent04()
    {
        if (is_setup == false)
        {
            player_start_pos = player.transform.position;

            is_setup = true;
        }
        zombiecount++;
        if (zombiecount / 60.0f <= 2)
        {
            player.transform.position = player_start_pos;
            Vector3 randompos = Random.insideUnitCircle * 0.05f;
            player.transform.Translate(randompos + new Vector3(0, 0, player_z));
            return 0;
        }
        else if (zombiecount == 60 * 2 + 1)
        {
            player.transform.Translate(0, 1, 0);
            talkmanager.startTalk("School/school_04");
            playercontroller.state = PlayerController.State.TALK;
        }

        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent05()
    {
        return talkNextEvent("School/school_05");
    }
    public int schoolEvent05_3()
    {
        return talkEvent("School/school_05-3");
    }
    public int schoolEvent06()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_06");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        if (talkmanager.selectbuttonnum == 1)
            playercontroller.setItem("Easel");
        is_setup = false;
        return 1;
    }
    public int schoolEvent06_2()//---------------------------------------------------------------------------------------------
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_06-1");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent07()
    {
        return talkNextEvent("School/school_07");
    }
    public int schoolEvent07_2()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_07-2");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent08()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_08");
            is_setup = true;
        }
        if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                talkmanager.event_call_count == 1)
        {
            SoundManager.Instance.volume.SE = 0.5f;
            SoundManager.Instance.PlaySE(2);
            talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 2;
    }
    public int schoolEvent08_2()
    {
        return talkEvent("School/school_08-2");
    }
    public int schoolEvent09()//------------------------------------------------------------------------------------
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_09");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 2;
    }
    public int schoolEvent09_2()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_09-2");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent11()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_11");
            is_setup = true;
        }
        if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                talkmanager.event_call_count == 1)
        {
            SoundManager.Instance.volume.SE = 0.5f;
            SoundManager.Instance.PlaySE(1);
            talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
        }
        if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                talkmanager.event_call_count == 2)
        {
            SoundManager.Instance.volume.SE = 0.5f;
            SoundManager.Instance.PlaySE(1);
            talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 2;
    }
    public int schoolEvent11_2()
    {
        return talkNextEvent("School/school_11-2");
    }
    public int schoolEvent11_3()
    {
        return talkEvent("School/school_11-3");
    }
    public int schoolEvent12()
    {
        return talkEvent("School/school_12");
    }
    public int schoolEvent13()
    {
        return talkEvent("School/school_13");
    }
    public int schoolEvent14()
    {
        return talkNextEvent("School/school_14");
    }
    public int schoolEvent14_2()
    {
        return talkNextEvent("School/school_14-2");
    }
    public int schoolEvent14_3()
    {
        return talkEvent("School/school_14-3");
    }
    public int schoolEvent15()//-------------------------------------------------------------------------------------
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_15");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent16()//----------------------------------------------------------------------------------------
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_16");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent17()
    {
        return talkEvent("School/school_17");
    }
    public int schoolEvent18()
    {
        return talkEvent("School/school_18");
    }//-------------------------------------------------------------------------------------------
    public int schoolEvent19()
    {
        return talkEvent("School/school_19");
    }
    public int schoolEvent20()
    {
        return talkNextEvent("School/school_20");
    }
    public int schoolEvent20_2()//------------------------------------------------------------------------------------------
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_20_2");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    public int schoolEvent21()
    {
        return talkEvent("School/school_21");
    }
    public int schoolEvent22()//----------------------------------------------------------------------------------------------
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("School/school_22");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        is_setup = false;
        return 1;
    }
    bool is_enemypop = false;
    public int schoolBossEvent()
    {
        if (is_enemypop == false)
        {
            if (staging.flushStart())
            {
                is_enemypop = true;
            }
            return 0;
        }
        else
        {
            if (staging.flushEnd())
            {
                if (is_setup == false)
                {
                    talkmanager.startTalk("School/school_23");
                    is_setup = true;
                }
                if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                     talkmanager.event_call_count >= 1)
                {
                    talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
                }
                if (talkmanager.is_talknow)
                    return 0;
                if (talkmanager.selectbuttonnum == 2)
                    enemymanager.nightmareRePop(talkmanager.selectbuttonnum);
                is_setup = false;
                staging.flushcount = 0;
                return 1;
            }
            return 0;
        }
    }


    /// イベントの作り方
    /// 各関数ごとにintの返り値がある
    /// return 0    イベント続行中
    ///        1    イベントを終了させるかつ、呼び出されるたびに同じイベントを繰り返す
    ///        2    イベントを終了させるかつ、イベントの段階を進める
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //  ヴィーデルの館イベント                                                                                                             //
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //int floor_layer = (int)LayerController.Layer.FLOOR;
    //int wall_layer = (int)LayerController.Layer.WALL;
    //int door_layer = (int)LayerController.Layer.DOOR;
    //int object_layer = (int)LayerController.Layer.OBJECT;
    int event_layer = (int)LayerController.Layer.EVENT;


    void roomMoveUpdate()
    {
        if (is_roommove)
            if (staging.fadeInBlack())
            {
                is_roommove = false;
            }
        if (Input.GetKeyDown(KeyCode.C))
        {
            eventrepository.houseEndEventSetup();
            eventloader.eventRegister();
        }
    }

    /// <summary>
    /// プレイヤーを引数で渡したイベントのマスに移動させる関数
    /// 移動させるときに、周囲４マスを調べて移動イベントの上や、壁ドア物がないところに移動させる
    /// </summary>
    bool roomMoveToPlayer(int gotoevent_)
    {
        for (int y = 0; y < chipcontroller.chip_num_y; y++)
        {
            for (int x = 0; x < chipcontroller.chip_num_x; x++)
            {
                if (chipcontroller.blockcomponents[event_layer][y][x].number == gotoevent_)
                {
                    if (x + 1 < chipcontroller.chip_num_x - 1 &&
                        chipcontroller.blockcomponents[event_layer][y][x + 1].number != gotoevent_ &&
                        chipcontroller.isFloor(x + 1, y))
                    {
                        chipcontroller.playerSelectCellPop(x + 1, y);
                        return true;
                    }
                    if (x - 1 > 0 &&
                        chipcontroller.blockcomponents[event_layer][y][x - 1].number != gotoevent_ &&
                        chipcontroller.isFloor(x - 1, y))
                    {
                        chipcontroller.playerSelectCellPop(x - 1, y);
                        return true;
                    }
                    if (y + 1 < chipcontroller.chip_num_y - 1 &&
                        chipcontroller.blockcomponents[event_layer][y + 1][x].number != gotoevent_ &&
                        chipcontroller.isFloor(x, y + 1))
                    {
                        chipcontroller.playerSelectCellPop(x, y + 1);
                        return true;
                    }
                    if (y - 1 > 0 &&
                        chipcontroller.blockcomponents[event_layer][y - 1][x].number != gotoevent_ &&
                        chipcontroller.isFloor(x, y - 1))
                    {
                        chipcontroller.playerSelectCellPop(x, y - 1);
                        return true;
                    }
                }
            }

        }
        return false;
    }

    public int houseEvent02()
    {
        if (SceneInfoManager.instance.endingstatus != SceneInfoManager.EndingStatus.NOT_END)
        {
            if (is_setup == false)
            {
                talkmanager.startTalk("Home/homeExit");
                is_setup = true;
            }
            if (talkmanager.is_talknow)
                return 0;
            if (talkmanager.selectbuttonnum == 1)
            {
                if (staging.fadeOutBlack())
                {
                    SoundManager.Instance.StopBGM();
                    SceneManager.LoadScene("Ending");
                    is_setup = false;
                    return 1;
                }
                return 0;
            }
        }
        return talkEvent("Home/home_2");
    }

    bool is_roommove = false;

    // 入る場合は自分の数字
    // 出る場合は出る先の数字より１少ない数字
    int moveToRoom(string room_name_, int eventnum_, string loadTexture_ = "Videl")
    {
        if (is_roommove == false)
            if (staging.fadeOutBlack())
            {
                chipcontroller.mapChange(room_name_, loadTexture_);
                eventloader.eventRegister();
                roomMoveToPlayer(eventnum_);
                is_roommove = true;
                return 1;
            }
        return 0;
    }
    // チュートリアルシナリオから帰ってきたときに呼ばれる関数
    void tutorialReturnToRoom()
    {
        chipcontroller.mapChange("House1FCenter", "Videl");
        eventloader.eventRegister();
        roomMoveToPlayer(7);
        is_roommove = true;
    }

    public int houseEvent03()
    {
        if (SceneInfoManager.instance.is_tutorial == true) return 1;
        return moveToRoom("House1FLeft", 3);
    }
    public int houseEvent04()
    {
        return moveToRoom("House1F", 2);
    }
    public int houseEvent05()
    {
        if (SceneInfoManager.instance.is_tutorial == true) return 1;
        return moveToRoom("House1FRight", 5);
    }
    public int houseEvent06()
    {
        return moveToRoom("House1F", 4);
    }
    /// <summary>
    /// 一番最初に書斎に入る時にチュートリアルを終了する
    /// </summary>
    public int houseEvent07()
    {
        if (SceneInfoManager.instance.is_tutorial)
        {
            int event_state = moveToRoom("House1FCenter", 7);
            if (event_state == 1)
            {
                tutorialEndScenarioChange();
                SceneManager.LoadScene("Scenario");
                return 1;
            }
            return event_state;
        }
        return moveToRoom("House1FCenter", 7);
    }
    public int houseEvent08()
    {
        return moveToRoom("House1F", 6);
    }
    public int houseEvent09()
    {
        return moveToRoom("House2F", 10);
    }
    public int houseEvent10()
    {
        return moveToRoom("House2F", 11);
    }
    public int houseEvent11()
    {
        return moveToRoom("House1F", 8);
    }
    public int houseEvent12()
    {
        return moveToRoom("House1F", 9);
    }

    public int houseEvent13()
    {
        return talkEvent("Home/home_13");
    }//-------------------------------------------------------------------------------------
    public int houseEvent14()
    {
        return talkEvent("Home/home_14");
    }//-------------------------------------------------------------------------------------
    public int houseEvent15()
    {
        return talkEvent("Home/home_15");
    }//-------------------------------------------------------------------------------------

    public int houseEvent16()
    {
        return moveToRoom("House2FLeftBook", 16);
    }
    public int houseEvent17()
    {
        return moveToRoom("House2F", 15);
    }
    public int houseEvent18()
    {
        return moveToRoom("House2FRightBook", 18);
    }
    public int houseEvent19()
    {
        return moveToRoom("House2F", 17);
    }

    //-------------------------------------------------------------------------------------
    public int houseEvent20()//-------------------------------------------------------------------------------------
    {
        return talkEvent("Home/home_20");
    }

    public int houseEvent21()
    {
        if (is_setup == false)
        {
            talkmanager.startTalk("Home/home_21");
            is_setup = true;
        }
        if (talkmanager.is_talknow)
            return 0;
        if (talkmanager.selectbuttonnum == 1)
        {
            if (is_roommove == false)
                if (staging.fadeOutBlack())
                {
                    eventrepository.schoolEventSetup();
                    chipcontroller.mapChange("school1", "School");
                    eventloader.eventRegister();
                    chipcontroller.playerPop();
                    enemymanager.enemySetup();
                    is_setup = false;
                    is_roommove = true;
                    return 1;
                }
            return 0;
        }
        is_setup = false;
        return 1;
    }
    public int houseEvent22()
    {
        return talkEvent("Home/home_22");
    }
    public int houseEvent22_2()
    {
        return talkEvent("Home/home_22-2");
    }
    public int houseEvent23()
    {
        return talkEvent("Home/home_23");
    }
    public int houseEvent24()
    {
        return talkEvent("Home/home_24");
    }
    public int houseEvent25()
    {
        return talkEvent("Home/home_25");
    }
    public int houseEvent25_2()
    {
        return talkEvent("Home/home_25-2");
    }
    public int houseEvent26()
    {
        return talkEvent("Home/home_26");
    }
    public int houseEvent26_2()
    {
        return talkEvent("Home/home_26-2");
    }
    public int houseEvent27()
    {
        return talkEvent("Home/home_27");
    }
    public int houseEvent27_2()
    {
        return talkEvent("Home/home_27-2");
    }
    public int houseEvent28()
    {
        return talkEvent("Home/home_28");
    }
    public int houseEvent28_2()
    {
        return talkEvent("Home/home_28-2");
    }
    public int houseEvent29()
    {
        return talkEvent("Home/home_29");
    }
    public int houseEvent29_2()
    {
        return talkEvent("Home/home_29-2");
    }
    public int houseEvent30()
    {
        return talkEvent("Home/home_30");
    }
    public int houseEvent31()
    {
        return talkEvent("Home/home_31");
    }
    public int houseEvent31_2()
    {
        return talkEvent("Home/home_31-2");
    }
    public int houseEvent32()
    {
        return talkEvent("Home/home_32");
    }
    public int houseEvent32_2()
    {
        return talkEvent("Home/home_32-2");
    }
    public int houseEvent33()
    {
        return talkEvent("Home/home_33");
    }
    public int houseEvent33_2()
    {
        return talkEvent("Home/home_33-2");
    }
    public int houseEvent34()
    {
        return talkEvent("Home/home_34");
    }
    public int houseEvent34_2()
    {
        return talkEvent("Home/home_34-2");
    }
    public int houseEvent35()
    {
        return talkEvent("Home/home_35");
    }
    public int houseEvent35_2()
    {
        return talkEvent("Home/home_35-2");
    }
    public int houseEvent36()
    {
        return talkEvent("Home/home_36");
    }
    public int houseEvent36_2()
    {
        return talkEvent("Home/home_36-2");
    }
    public int houseEvent37()
    {
        return talkEvent("Home/home_37");
    }
    public int houseEvent37_2()
    {
        return talkEvent("Home/home_37-2");
    }
    public int houseEvent38()
    {
        return talkEvent("Home/home_38");
    }
    public int houseEvent38_2()
    {
        return talkEvent("Home/home_38-2");
    }
    public int houseEvent39()
    {
        return talkEvent("Home/home_39");
    }
    public int houseEvent39_2()
    {
        return talkEvent("Home/home_39-2");
    }
    public int houseEvent40()
    {
        return talkEvent("Home/home_40");
    }
    public int houseEvent41()
    {
        return talkEvent("Home/home_41");
    }
    public int houseEvent41_2()
    {
        return talkEvent("Home/home_41-2");
    }
    public int houseEvent42()
    {
        return talkEvent("Home/home_42");
    }
    public int houseEvent42_2()
    {
        return talkEvent("Home/home_42-2");
    }
    public int houseEvent43()
    {
        return talkEvent("Home/home_43");
    }
    public int houseEvent43_2()
    {
        return talkEvent("Home/home_43-2");
    }
    public int houseEvent44()
    {
        return talkEvent("Home/home_44");
    }
    public int houseEvent44_2()
    {
        return talkEvent("Home/home_44-2");
    }
    public int houseEvent45()
    {
        return talkEvent("Home/home_45");
    }
    public int houseEvent45_2()
    {
        return talkEvent("Home/home_45-2");
    }
    public int houseEvent46()
    {
        return talkEvent("Home/home_46");
    }
    public int houseEvent46_2()
    {
        return talkEvent("Home/home_46-2");
    }
    public int houseEvent47()
    {
        return talkEvent("Home/home_47");
    }
    public int houseEvent47_2()
    {
        return talkEvent("Home/home_47-2");
    }
    public int houseEvent48()
    {
        return talkEvent("Home/home_48");
    }
    public int houseEvent48_2()
    {
        return talkEvent("Home/home_48-2");
    }

    public int houseEvent49()
    {
        if (SceneInfoManager.instance.is_shoolclear)
        {
            if (is_setup == false)
            {
                talkmanager.startTalk("Home/ed1Nearness");
                is_setup = true;
            }
            if (talkmanager.talkmode == EventTalkManager.TalkMode.EVENT &&
                     talkmanager.event_call_count == 1)
            {
                SoundManager.Instance.volume.SE = 0.5f;
                SoundManager.Instance.PlaySE(2);
                talkmanager.talkmode = EventTalkManager.TalkMode.NORMAL;
            }
            if (talkmanager.is_talknow)
                return 0;
            if (talkmanager.selectbuttonnum == 1)
            {
                endingPartingpoint();
                endEventSetup();
                return 1;
            }
            is_setup = false;
            return 1;
        }
        int rand = Random.Range(1, 6);
        return talkEvent("Home/home_49-" + rand.ToString());
    }
    public int houseEvent49_2()
    {
        int rand = Random.Range(7, 11);
        return talkEvent("Home/home_49-" + rand.ToString());
    }

    /// <summary>
    /// ナイトメアを渡した後のイベントに変更する関数
    /// </summary>
    void endEventSetup()
    {
        eventrepository.houseEndEventSetup();
        eventloader.eventRegister();
    }
    /// <summary>
    /// どちらのエンディングか判定する関数
    /// </summary>
    void endingPartingpoint()
    {
        if (SceneInfoManager.instance.school_enemy_num <= 0)
        {
            SceneInfoManager.instance.endingstatus = SceneInfoManager.EndingStatus.GOOD_END;
        }
        else if (SceneInfoManager.instance.school_enemy_num >= 1)
        {
            SceneInfoManager.instance.endingstatus = SceneInfoManager.EndingStatus.HUNGRY_END;
        }
    }
}
