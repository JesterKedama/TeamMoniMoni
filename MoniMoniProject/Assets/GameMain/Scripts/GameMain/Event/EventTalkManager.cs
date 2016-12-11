﻿using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;

/// <summary>
/// イベントが起こった時に話をするところのスクリプト
/// </summary>
public class EventTalkManager : MonoBehaviour
{
    [SerializeField]
    PlayerController player;


    [SerializeField]
    Text nametext;
    [SerializeField]
    Text talktext;
    [SerializeField]
    Image charaimage1;

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
        audiosource.Play();
    }


    public bool is_talknow;

    // 会話の種類
    public enum TalkMode
    {
        NORMAL,
        SELECT,
    }
    public TalkMode talkmode;

    // テキストのパス
    string loadtextpath;
    // 読んだテキストの中身全部
    string loadtextdata;
    // 会話の時に表示される名前
    string draw_name;
    // 会話の時に表示される文
    string draw_talk;
    // 会話の時に表示されるキャラの画像
    string texturename;

    Sprite[] sprites;

    int current_read_line;

    /// <summary>
    /// イベントの時に会話を呼ぶための関数
    /// </summary>
    /// <param name="textname_"></param>
    public void startTalk(string textname_)
    {
        loadtextpath = textname_;
        // 会話してなかったらテキストを読む
        if (is_talknow == false)
        {
            is_talknow = true;
            rootButtonSetup();
            current_read_line = 0;
            using (var sr = new StreamReader(Application.dataPath + "/GameMain/Resources/EventData/" + textname_ + ".txt"))
            {
                loadtextdata = sr.ReadToEnd();
            }
            loadTalk(textname_);
            player.state = PlayerController.State.TALK;
        }
    }

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
            draw_talk = null;
            texturename = null;

            textDataCheck(loadtextdata);

            nametext.text = draw_name;
            talktext.text = draw_talk;

            charaimage1.sprite =
                System.Array.Find<Sprite>(
                                    sprites, (sprite) => sprite.name.Equals(
                                        texturename));
            if (charaimage1.sprite == null)
            {
                charaimage1.sprite = System.Array.Find<Sprite>(
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

            if (talkmode == TalkMode.NORMAL)
            {
                // コマンドを探すswitch文
                switch (command)
                {
                    case "p":
                        draw_talk += "\n";
                        continue;
                    case "n":
                        current_read_line = i;
                        return;
                    case "end":
                        is_talknow = false;
                        return;
                }

                if (command != null)
                {
                    // キャラを表示させるコマンドが来たら通る
                    if (command.IndexOf("chara") != -1)
                    {
                        texturename = commandPickOutTextureName(command);
                    }
                    // エフェクトが来たら通る
                    if (command.IndexOf("effect") != -1)
                    {

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
                draw_talk += chara_array[i];
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
    string commandPickOutTextureName(string command_)
    {
        char[] c = command_.ToCharArray();
        char texturestart = '\'';
        bool is_texturestart = false;
        string texturename = null;

        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == texturestart)
            {
                is_texturestart = !is_texturestart;
                continue;
            }
            if (is_texturestart)
            {
                texturename += c[i];
            }
        }
        return texturename;
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

    AudioClip audioclip;
    AudioSource audiosource;

    void Start()
    {
        is_talknow = false;
        sprites = Resources.LoadAll<Sprite>("Textures/Talk");
        audiosource = GetComponent<AudioSource>();
        //audiosource.clip = audioclip;
    }

    void Update()
    {
        if (player.state == PlayerController.State.TALK)
        {
            if (is_talknow)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (is_selectbuttonpush)
                        loadTalk(loadtextpath);
                }
                if (is_talknow == false)
                {
                    player.state = PlayerController.State.NORMAL;
                }
            }
        }
    }

}
