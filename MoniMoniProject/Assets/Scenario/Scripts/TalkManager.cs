using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class TalkManager : MonoBehaviour
{
    [SerializeField]
    GameObject talkwindow;

    [SerializeField]
    Text nametext;

    [SerializeField]
    Image charaimage1;
    [SerializeField]
    Image charaimage2;

    // 使う背景全部
    [SerializeField]
    Sprite night_town;
    [SerializeField]
    Sprite room;
    [SerializeField]
    Sprite forest;
    [SerializeField]
    Sprite building;

    [SerializeField]
    Image background;

    [SerializeField]
    Button root1button;
    [SerializeField]
    Button root2button;
    [SerializeField]
    Button root3button;
    [SerializeField]
    Button root4button;

    public int selectbuttonnum;
    // セレクトボタンが出て、押してない場合を判定する変数
    public bool is_selectbuttonpush;

    public void selectRoot1()
    {
        selectbuttonnum = 1;
        rootButtonSetup();
        rootSelectSoundPlay();
    }
    public void selectRoot2()
    {
        selectbuttonnum = 2;
        rootButtonSetup();
        rootSelectSoundPlay();
    }
    public void selectRoot3()
    {
        selectbuttonnum = 3;
        rootButtonSetup();
        rootSelectSoundPlay();
    }
    public void selectRoot4()
    {
        selectbuttonnum = 4;
        rootButtonSetup();
        rootSelectSoundPlay();
    }
    void rootButtonSetup()
    {
        root1button.gameObject.SetActive(false);
        root2button.gameObject.SetActive(false);
        root3button.gameObject.SetActive(false);
        root4button.gameObject.SetActive(false);
        is_selectbuttonpush = true;
    }
    void rootSelectSoundPlay()
    {
        se_audiosource.Play();
    }

    [SerializeField]
    GameObject talktext;
    // 文字の基準位置
    Vector3 talkstartpos;
    // 文字の今の位置
    Vector3 talkcurrentpos;

    [SerializeField]
    int font_defaultsize;
    [SerializeField]
    int font_bigsize;
    int fontsize;
    Color fontcolor;

    public bool is_talknow;

    public bool is_event;

    // 会話の種類
    public enum TalkMode
    {
        NORMAL,
        SELECT,
        EVENT,
    }
    public TalkMode talkmode;

    // テキストのパス
    string loadtextpath;
    // 読んだテキストの中身全部
    string loadtextdata;
    // 会話の時に表示される名前
    string draw_name;
    // 会話の時に表示されるキャラの画像
    string texturename1;
    string texturename2;

    Sprite[] sprites;

    int current_read_line;

    /// <summary>
    /// テキストファイルを開いて名前と会話を取り出す関数
    /// </summary>
    /// <param name="textname_"></param>
    public void loadTalk(string textname_)
    {
        // 会話していたら呼ばれるたびに会話文と名前を更新する
        if (is_talknow)
        {
            draw_name = null;
            texturename1 = null;
            texturename2 = null;
            talkTextClear();
            textDataCheck(loadtextdata);

            nametext.text = draw_name;


            charaimage1.sprite =
                System.Array.Find<Sprite>(
                                    sprites, (sprite) => sprite.name.Equals(
                                        texturename1));
            charaimage2.sprite =
                System.Array.Find<Sprite>(
                                    sprites, (sprite) => sprite.name.Equals(
                                        texturename2));

            if (charaimage1.sprite == null)
            {
                charaimage1.sprite = System.Array.Find<Sprite>(
                                    sprites, (sprite) => sprite.name.Equals(
                                        "none"));

            }
            if (charaimage2.sprite == null)
            {
                charaimage2.sprite =
                System.Array.Find<Sprite>(
                                    sprites, (sprite) => sprite.name.Equals(
                                        "none"));
            }

        }
    }

    /// <summary>
    /// 渡されたテキストを分解して表示する文字列に保存する関数
    /// </summary>
    /// <param name="loadtext_">読み込んだ文字列</param>
    void textDataCheck(string loadtext_)
    {
        char[] chara_array = loadtext_.ToCharArray();

        int texturecount = 0;
        for (int i = current_read_line; i < chara_array.Length; i++)
        {
            string command = null;

            // メモ書きの判定
            if (chara_array[i] == '/')
            {
                if (chara_array[i + 1] == '/')
                {
                    while (true)
                    {
                        i++;
                        if (chara_array[i] == '\n')
                        {
                            break;
                        }
                    }
                    continue;
                }
            }

            // コマンド開始
            if (chara_array[i] == '[')
            {
                command = commandSearch(loadtext_, i);
                if (command != "end")
                    i += command.Length + 2;
            }
            // 名前開始
            else if (chara_array[i] == '#')
            {
                draw_name = commandSearch(loadtext_, i);
                if (draw_name != null)
                    i += draw_name.Length + 1;
                else
                {
                    draw_name = " ";
                    for (int k = i + 1; k < chara_array.Length; k++)
                    {
                        if (chara_array[k] == '#')
                        {
                            i = k;
                            break;
                        }
                    }
                }
                continue;
            }

            // 会話の種類 (普通の会話や選択肢など)
            if (command == "text")
            {
                talkmode = TalkMode.NORMAL;
            }
            else if (command == "root")
            {
                talkmode = TalkMode.SELECT;
            }
            else if (command == "event")
            {
                talkmode = TalkMode.EVENT;
            }

            if (talkmode == TalkMode.NORMAL)
            {
                // コマンドを探すswitch文
                switch (command)
                {
                    case "p":
                        talkCharInstance('\n', font_defaultsize, Color.white);
                        continue;
                    case "n":
                        current_read_line = i;
                        return;
                    case "end":
                        talkTextClear();
                        SceneManager.LoadScene("GameMain");
                        return;
                }

                if (command != null)
                {
                    // キャラを表示させるコマンドが来たら通る
                    if (command.IndexOf("chara") != -1)
                    {
                        if (texturecount == 0)
                            texturename1 = commandPickOutName(command);
                        else if (texturecount == 1)
                            texturename2 = commandPickOutName(command);
                        texturecount++;
                    }
                    // エフェクトが来たら通る
                    if (command.IndexOf("effect") != -1)
                    {

                    }

                    string pickoutcommand = null;
                    // サイズを変えるコマンドが来たら通る
                    if (command.IndexOf("size") != -1)
                    {
                        pickoutcommand = commandPickOutName(command);
                        if (pickoutcommand == "start")
                            fontsize = font_bigsize;
                        else if (pickoutcommand == "end")
                            fontsize = font_defaultsize;
                    }

                    // 色変えるコマンドが来たら通る
                    bool is_colorchange = false;
                    Color changecolor = Color.black;
                    if (command.IndexOf("red") != -1)
                    {
                        changecolor = Color.red;
                        is_colorchange = true;
                    }
                    if (is_colorchange)
                    {
                        pickoutcommand = commandPickOutName(command);
                        if (pickoutcommand == "start")
                            fontcolor = changecolor;
                        else if (pickoutcommand == "end")
                            fontcolor = Color.black;
                    }
                }

                if (chara_array[i] == '(')
                {
                    string rootnum_st = commandSearch(loadtext_, i);
                    i += rootnum_st.Length + 1;
                    int rootnum = int.Parse(rootnum_st);

                    if (rootnum == selectbuttonnum)
                    {
                        while (true)
                        {
                            i++;
                            if (chara_array[i] == '{')
                                break;
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            i++;
                            if (chara_array[i] == '}')
                                break;
                        }
                    }
                    continue;
                }

                if (chara_array[i] == ' ' ||
                    chara_array[i] == '\r' ||
                    chara_array[i] == '\n') continue;

                // 会話文に追加
                talkCharInstance(chara_array[i], fontsize, fontcolor);
            }
            if (talkmode == TalkMode.SELECT)
            {
                string rootcommand = null;
                if (chara_array[i] == '(')
                {
                    rootcommand = commandSearch(loadtext_, i);

                    rootButtonSetting(rootcommand);

                    i += rootcommand.Length + 2;
                    is_selectbuttonpush = false;
                    continue;
                }
            }

            if (talkmode == TalkMode.EVENT)
            {
                current_read_line = i;
                return;
            }
        }

    }

    /// <summary>
    /// コマンドを取り出す関数
    /// </summary>
    /// <param name="loadtext_">テキストから読み込んだ文字列</param>
    /// <param name="currentcharapos_">読んでいる位置</param>
    /// <returns>コマンド(文字列)</returns>
    string commandSearch(string loadtext_, int currentcharapos_)
    {
        // char配列にテキスト入れる
        char[] c = loadtext_.ToCharArray();
        // 位置を保存
        int i = currentcharapos_;

        string command = null;

        // コマンドが終わったらループ終了
        while (true)
        {
            i++;
            if (c[i] == ' ') continue;
            if (c[i] == ']' || c[i] == '#' || c[i] == ')') break;
            command += c[i];
        }

        return command;
    }

    /// <summary>
    /// コマンドの中からtextureの情報を引き出す関数
    /// </summary>
    /// <param name="command_"></param>
    /// <returns></returns>
    public string commandPickOutName(string command_)
    {
        char[] c = command_.ToCharArray();
        char start = '\'';
        bool is_start = false;
        string name = null;

        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == start)
            {
                is_start = !is_start;
                continue;
            }
            if (is_start)
            {
                name += c[i];
            }
        }
        return name;
    }

    /// <summary>
    /// 会話画面に文字をインスタンスする関数
    /// </summary>
    /// <param name="drawchar_"></param>
    /// <param name="fontsize_"></param>
    /// <param name="fontcolor_"></param>
    public void talkCharInstance(char drawchar_, int fontsize_, Color fontcolor_)
    {
        var drawchar = Resources.Load<GameObject>("Prefabs/TalkChar");
        var text = drawchar.GetComponent<Text>();

        text.text = drawchar_.ToString();
        text.color = fontcolor_;
        text.fontSize = fontsize_;
        if (drawchar_ == '\n')
        {
            talkcurrentpos.x = talkstartpos.x;
            talkcurrentpos.y -= fontsize_ + 10;
        }
        else
        {
            talkcurrentpos.x += fontsize_ + 6;
        }
        drawchar = (GameObject)Instantiate(drawchar, talktext.transform);
        drawchar.transform.localPosition = talkcurrentpos;
        drawchar.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    /// <summary>
    /// 会話の文字をクリアする関数
    /// </summary>
    public void talkTextClear()
    {
        if (talktext.transform.childCount >= 0)
            foreach (Transform child in talktext.transform)
            {
                Destroy(child.gameObject);
            }

        talktext.transform.localPosition = new Vector3(0, -230, 0);
        talkstartpos = Vector3.zero;
        talkcurrentpos = talkstartpos;
        fontsize = font_defaultsize;
        fontcolor = Color.black;
    }

    /// <summary>
    /// 選択肢のコマンドの中身を調べて選択肢を表示させる関数
    /// </summary>
    /// <param name="rootcommand_"></param>
    void rootButtonSetting(string rootcommand_)
    {
        int rootcount = 0;

        bool is_countget = true;
        int textcount = 0;
        for (int i = 0; i < rootcommand_.Length; i++)
        {
            char[] root_c = rootcommand_.ToCharArray();
            if (root_c[i] == ' ') continue;

            if (is_countget)
            {
                rootcount = int.Parse(root_c[i].ToString());
                is_countget = false;
                while (true)
                {
                    i++;
                    if (root_c[i] == ',')
                        break;
                }
            }
            else
            {
                string buttontext = null;

                while (true)
                {
                    if (i >= rootcommand_.Length
                        || root_c[i] == ',')
                    {
                        textcount += 1;
                        break;
                    }

                    if (root_c[i] == ' ') continue;
                    buttontext += root_c[i];
                    i++;
                }

                Text text = null;
                if (textcount == 1)
                {
                    root1button.gameObject.SetActive(true);
                    text = root1button.transform.FindChild("Text").GetComponent<Text>();
                }
                if (textcount == 2)
                {
                    root2button.gameObject.SetActive(true);
                    text = root2button.transform.FindChild("Text").GetComponent<Text>();
                }
                if (textcount == 3)
                {
                    root3button.gameObject.SetActive(true);
                    text = root3button.transform.FindChild("Text").GetComponent<Text>();
                }
                if (textcount == 4)
                {
                    root4button.gameObject.SetActive(true);
                    text = root4button.transform.FindChild("Text").GetComponent<Text>();
                }

                text.text = buttontext;

                if (textcount >= rootcount)
                    break;
            }
        }
    }

    [SerializeField]
    AudioSource bgm_audiosource;

    [SerializeField]
    AudioSource se_audiosource;

    [SerializeField]
    AudioClip select_se;
    [SerializeField]
    AudioClip forest_bgm;
    [SerializeField]
    AudioClip hungry_se;
    [SerializeField]
    AudioClip building_bgm;

    [SerializeField]
    AudioClip footsteps_se;
    [SerializeField]
    AudioClip forest_se;

    [SerializeField]
    AudioClip doorclose_se;
    [SerializeField]
    AudioClip dooropen_se;

    [SerializeField]
    StagingController staging;
    [SerializeField]
    GameObject nowloadingtexture;
    [SerializeField]
    GameObject stagingcanvas;

    int currentevent;

    void prologueEventModeUpdate()
    {
        if (bgm_audiosource.isPlaying == false)
        {
            if (currentevent <= 1)
            {
                bgm_audiosource.clip = forest_bgm;
                bgm_audiosource.Play();
            }
            if (currentevent == 4)
            {
                bgm_audiosource.clip = building_bgm;
                bgm_audiosource.Play();
            }
        }
        if (talkmode != TalkMode.EVENT) return;

        // おなか鳴らす
        if (currentevent == 0)
        {
            talkTextClear();
            se_audiosource.clip = hungry_se;
            se_audiosource.Play();
            talkmode = TalkMode.NORMAL;
            talkwindow.SetActive(false);
            currentevent++;
            return;
        }

        // おいしそうなにおいが漂う夜の街に移動
        if (currentevent == 1)
        {
            talkwindow.SetActive(false);
            if (staging.fadeOutBlack())
            {
                background.sprite = night_town;
                currentevent++;
            }
        }
        if (currentevent == 2)
        {
            if (staging.fadeInBlack())
            {
                talkmode = TalkMode.NORMAL;
                currentevent++;
                return;
            }
        }

        // 森に移動
        if (currentevent == 3)
        {
            talkwindow.SetActive(false);
            if (staging.fadeOutBlack())
            {
                background.sprite = forest;
                se_audiosource.clip = footsteps_se;
                se_audiosource.Play();
                currentevent++;
            }
        }
        if (currentevent == 4)
        {
            if (staging.fadeInBlack())
            {
                talkmode = TalkMode.NORMAL;
                currentevent++;
                return;
            }
        }

        // 森がざわつく音
        if (currentevent == 5)
        {
            talkwindow.SetActive(false);
            se_audiosource.clip = forest_se;
            se_audiosource.Play();
            talkmode = TalkMode.NORMAL;
            currentevent++;
            return;
        }

        // おなかを鳴らす
        if (currentevent == 6)
        {
            talkTextClear();
            se_audiosource.clip = hungry_se;
            se_audiosource.Play();
            talkmode = TalkMode.NORMAL;
            talkwindow.SetActive(false);
            currentevent++;
            return;
        }

        // 怪しげな洋館外観を見に行く
        if (currentevent == 7)
        {
            talkwindow.SetActive(false);
            if (staging.fadeOutBlack())
            {
                background.sprite = building;
                se_audiosource.clip = footsteps_se;
                se_audiosource.Play();
                currentevent++;
            }
        }
        if (currentevent == 8)
        {
            if (staging.fadeInBlack())
            {
                talkmode = TalkMode.NORMAL;
                currentevent++;
                return;
            }
        }

        // 館に移動
        if (currentevent == 9)
        {
            talkwindow.SetActive(false);
            if (staging.fadeOutBlack())
            {
                background.sprite = room;
                bgm_audiosource.clip = building_bgm;
                bgm_audiosource.Play();
                se_audiosource.clip = dooropen_se;
                se_audiosource.Play();
                currentevent++;
            }
        }
        if (currentevent == 10)
        {
            if (staging.fadeInBlack())
            {
                talkmode = TalkMode.NORMAL;
                currentevent++;
                return;
            }
        }
        // ドアが閉まる
        if (currentevent == 11)
        {
            talkTextClear();
            se_audiosource.clip = doorclose_se;
            se_audiosource.Play();
            talkmode = TalkMode.NORMAL;
            currentevent++;
            return;
        }

        // ゲームメインに飛ぶ
        if (currentevent == 12)
        {
            if (staging.fadeOutBlack())
            {
                nowloadingtexture.SetActive(true);
                talkmode = TalkMode.NORMAL;
                currentevent++;
                SceneManager.LoadScene("GameMain");
                return;
            }
        }



    }

    void prologue2EventModeUpdate()
    {
        if (bgm_audiosource.isPlaying == false)
        {
            if (currentevent <= 1)
            {
                bgm_audiosource.clip = building_bgm;
                bgm_audiosource.Play();
            }
        }

        if (talkmode != TalkMode.EVENT) return;

        // 経緯を話す
        if (currentevent == 0)
        {
            var color = background.color;
            color.r -= 0.03f;
            color.g -= 0.03f;
            color.b -= 0.03f;
            background.color = color;
            if (color.r <= 0)
            {
                talkmode = TalkMode.NORMAL;
                currentevent++;
                return;
            }
        }

        if (currentevent == 1)
        {
            var color = background.color;
            color.r += 0.03f;
            color.g += 0.03f;
            color.b += 0.03f;
            background.color = color;
            if (color.r >= 1.0f)
            {
                talkmode = TalkMode.NORMAL;
                currentevent++;
                return;
            }
        }
        // ゲームメインに飛ぶ
        if (currentevent == 2)
        {
            if (staging.fadeOutBlack())
            {
                nowloadingtexture.SetActive(true);
                talkmode = TalkMode.NORMAL;
                currentevent++;
                SceneManager.LoadScene("GameMain");
                return;
            }
        }
    }


    void Start()
    {
        is_talknow = false;
    }

    public void talkStart()
    {
        is_talknow = false;
        is_selectbuttonpush = true;
        sprites = Resources.LoadAll<Sprite>("Textures/Talk");

        if (SceneInfoManager.instance.is_tutorial)
        {
            bgm_audiosource.clip = forest_bgm;
            bgm_audiosource.Play();
            loadtextpath = "Prologue";
        }
        else
        {
            bgm_audiosource.clip = building_bgm;
            bgm_audiosource.Play();
            loadtextpath = "Prologue2";
            background.sprite = room;
        }

        var scenariotext = Resources.Load<TextAsset>("TextData/" + loadtextpath);

        using (var sr = new StringReader(scenariotext.text))
        {
            loadtextdata = sr.ReadToEnd();
        }
        loadTalk(loadtextpath);
        currentevent = 0;

        talkTextClear();
    }

    void Update()
    {
        if (is_talknow)
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (is_selectbuttonpush)
                {
                    talkwindow.SetActive(true);
                    loadTalk(loadtextpath);
                }
            }
            if (SceneInfoManager.instance.is_tutorial)
                prologueEventModeUpdate();
            else
            {
                prologue2EventModeUpdate();
            }

            if (Input.GetKey(KeyCode.Return))
            {
                stagingcanvas.SetActive(true);
                nowloadingtexture.SetActive(true);
                talkmode = TalkMode.NORMAL;
                SceneManager.LoadScene("GameMain");
            }
        }


    }
}
