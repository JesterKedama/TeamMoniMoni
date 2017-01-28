using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System;


public class OpeningTextController : MonoBehaviour
{
    [SerializeField]
    Text text;

    [SerializeField]
    int font_defaultsize;
    [SerializeField]
    int font_bigsize;
    int fontsize;
    Color fontcolor;

    string loadtextdata;
    string loadtextpath;
    int current_read_line;

    string draw_text;
    public bool is_talknow;

    float textalpha;

    void textDataCheck()
    {
        char[] chara_array = loadtextdata.ToCharArray();


        for (int i = current_read_line; i < chara_array.Length; i++)
        {
            string command = null;

            // メモ書きの判定
            if (chara_array[i] == '/')
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

            if (chara_array[i] == '[')
            {
                command = commandSearch(loadtextdata, i);
                if (command != "end")
                    i += command.Length + 2;
            }

            // コマンドを探すswitch文
            switch (command)
            {
                case "p":
                    draw_text += "\n";
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
                Color changecolor = Color.white;
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
                }
            }

            if (chara_array[i] == ' ' ||
                   chara_array[i] == '\r' ||
                   chara_array[i] == '\n') continue;

            draw_text += chara_array[i];
        }

    }

    void loadText()
    {
        if (is_talknow)
        {
            draw_text = null;
            textDataCheck();

            text.text = draw_text;
        }
    }

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

    [SerializeField]
    AudioSource audiosource;

    void Start()
    {
        loadtextpath = "Opening";

        var textdata = Resources.Load<TextAsset>("TextData/" + loadtextpath);


        using (var sr = new StringReader(textdata.text))
        {
            loadtextdata = sr.ReadToEnd();
        }

        textalpha = -0.1f;
        is_talknow = true;
        loadText();

        fademode = FadeMode.IN;

        fontsize = font_defaultsize;
        text.fontSize = fontsize;
        fontcolor = Color.white;
        audiosource.Play();
    }


    enum FadeMode
    {
        IN,
        OUT,
        DRAW,
        LOAD,
    }

    FadeMode fademode;

    void Update()
    {
        text.fontSize = fontsize;


        if (fademode == FadeMode.IN)
        {
            textalpha += 0.04f;
            if (Input.GetMouseButtonUp(0))
            {
                textalpha = 1.0f;
            }
            if (textalpha >= 1.0f)
            {
                fademode = FadeMode.DRAW;
            }
        }
        else if (fademode == FadeMode.DRAW)
        {
            if (Input.GetMouseButtonUp(0))
            {
                fademode = FadeMode.OUT;
            }
        }
        else if (fademode == FadeMode.OUT)
        {
            textalpha -= 0.02f;
            if (Input.GetMouseButtonUp(0))
            {
                fontcolor = Color.white;
                loadText();
                fademode = FadeMode.IN;
            }
            if (textalpha <= -0.2f)
            {
                fademode = FadeMode.LOAD;
            }
        }
        else if (fademode == FadeMode.LOAD)
        {
            fontcolor = Color.white;
            loadText();
            fademode = FadeMode.IN;
        }

        text.color = fontcolor;
        var color = text.color;
        color.a = textalpha;
        text.color = color;
        textalpha = Mathf.Clamp(textalpha, -1.0f, 1.0f);

        // スキップする処理
        if (Input.GetKeyDown(KeyCode.Return))
        {
            is_talknow = false;
        }

        if (SceneInfoManager.instance.is_tutorial == false)
        {
            is_talknow = false;
        }
    }
}
