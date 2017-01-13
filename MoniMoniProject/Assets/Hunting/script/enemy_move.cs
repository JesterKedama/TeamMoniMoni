﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class enemy_move : MonoBehaviour
{
    public int backgound_number;
    public Sprite school_1;
    public Sprite school_2;
    public Sprite school_boss;
    public Sprite hospital_1;
    public Sprite hospital_2;
    public Sprite hospital_boss;


    int Anime_one_number; //１回目に行う動きの番号
    int Anime_two_number;//２回目に行う動きの番号　１回目と同じ番号が出たら振り直し
    int Anime_three_number;//３回目に行う動きの番号　１，２回目と同じ番号が出たら振り直し
    int Anime_number; //現在の動き
    [SerializeField]
    Image background;

    public float start_time;

    [SerializeField]
    Image white_image;
    [SerializeField]
    private GameObject walker;
    walk_eat walker_move;
    [SerializeField]
    private GameObject enemy_HP_text;
    Text HP_text;
    [SerializeField]
    private GameObject enemy_HP_text_slot;
    Text HP_text_slot;
    

    [SerializeField]
    private GameObject BGMer;
    AudioSource touch_sound;

    public int time;
    public int warp_time;
    private int touch_count;
    public bool enemy_eat;
    private Vector3 enemy_pos;
    float anime_pos_x;
    float anime_pos_y;

    private Vector3 enemy_end_pos;
    private Vector3 animation_eat_end_pos = new Vector3(3.0f, 0.5f, -6);
    private float size;
    float speed;
    float speed_regulation;

    [SerializeField]
    private Collider2D _collider2D = null;
    // Use this for initialization
    void Start()
    {
        Anime_one_number = Random.Range(1, 9);
        Anime_two_number = 0;
        Anime_three_number = 0;
        switch (backgound_number)
        {
            case 1:
             background.sprite = school_1;
                break;
            case 2:
                background.sprite = school_2;
                break;
            case 3:
                background.sprite = school_boss;
                break;
            case 4:
                background.sprite = hospital_1;
                break;
            case 5:
                background.sprite = hospital_2;
                break;
            case 6:
                background.sprite = hospital_boss;
                break;
        }
        start_time = 0.0f;

        touch_sound = BGMer.GetComponent<AudioSource>();
        walker_move = walker.GetComponent<walk_eat>();
        HP_text = enemy_HP_text.GetComponent<Text>();
        HP_text_slot = enemy_HP_text_slot.GetComponent<Text>();
        size = 1.0f;
        transform.localScale = new Vector3(1, 1, 1);
        time = 0;
        warp_time = 0;
        touch_count = 0;
        enemy_eat = false;
        enemy_pos = Vector3.zero;
        enemy_end_pos = Vector3.zero;
        speed = 0;
        speed_regulation = 3;
        touch_sound.Play();
        touch_sound.loop = true;
        anime_pos_x = -5.0f;
        anime_pos_y = -3.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Anime_two_number == Anime_one_number) Anime_two_number = Random.Range(1, 9);
        if (Anime_three_number == Anime_one_number || Anime_three_number == Anime_two_number) Anime_three_number = Random.Range(1, 9);
        

        start_time++;
        if (start_time < 100)
        {
            transform.localPosition = new Vector3(30, 0, 0);
            var color = white_image.color;
            color.a = 1.0f - start_time / 100;
            white_image.color = color;
        }
        if (start_time == 100) {
            HP_text.text = "3";
            HP_text_slot.text = "あと    回敵をタップしてね";
                }
        if (start_time >= 100)
        {
            switch (Anime_number) {//x=-5~5 y=-3.2~3.2 モーション切り替えすべし
                case 1:
                    transform.localPosition = enemy_pos;
                    if (time < 150 && speed_regulation < 5.0f) speed_regulation += 0.04f;
                    if (time >= 150 && speed_regulation >= 1.0f) speed_regulation -= 0.04f;
                    speed += 0.03f * speed_regulation;
                    anime_pos_x += 0.05f;
                    if (anime_pos_x >= 5.0f) anime_pos_x = -5.0f;
                    enemy_pos = new Vector3(anime_pos_x, Mathf.Cos(-speed) * 1.8f, 0);
                    break;
                case 2:
                    transform.localPosition = enemy_pos;
                    if (time < 150 && speed_regulation < 5.0f) speed_regulation += 0.04f;
                    if (time >= 150 && speed_regulation >= 1.0f) speed_regulation -= 0.04f;
                    speed += 0.03f * speed_regulation;
                    enemy_pos = new Vector3(Mathf.Sin(-speed / 4) * 2, Mathf.Cos(-speed) * 1.8f, 0);
                    break;
                case 3:
                    transform.localPosition = enemy_pos;
                    if (time < 150 && speed_regulation < 5.0f) speed_regulation += 0.04f;
                    if (time >= 150 && speed_regulation >= 1.0f) speed_regulation -= 0.04f;
                    speed += 0.03f * speed_regulation;
                    enemy_pos = new Vector3(Mathf.Sin(-speed / 4) * 2, Mathf.Cos(-speed) * 1.8f, 0);
                    break;
            }

            time++;
            switch (touch_count)
            {
                case 0:
                    transform.localPosition = enemy_pos;
                    if (time < 150&&speed_regulation < 3.0f) speed_regulation += 0.04f;
                    if (time >= 150&&speed_regulation >= 1.0f)speed_regulation -= 0.04f;
                    speed += 0.05f * speed_regulation;
                    enemy_pos = new Vector3(Mathf.Sin(-speed) * 2.0f, Mathf.Cos(-speed) * 1.5f, 0);
                    break;
                case 1:
                    transform.localPosition = enemy_pos;
                    if (time < 150 && speed_regulation < 5.0f) speed_regulation += 0.04f;
                    if (time >= 150&&speed_regulation >= 1.0f)speed_regulation -= 0.04f;                    
                    speed += 0.03f * speed_regulation;
                    enemy_pos = new Vector3(Mathf.Sin(-speed / 4) * 2, Mathf.Cos(-speed) * 1.8f, 0);
                    break;
                case 2:
                    transform.localPosition = enemy_pos;
                    if (time < 200)warp_time += 6;
                    if (time >= 200&&time<500)warp_time++;
                    if (time >= 500 && time % 2 == 0) warp_time++;
                     speed += 0.03f * speed_regulation;
                    if (warp_time >= 40)
                    {
                        enemy_pos = new Vector3(Random.Range(-2.5f, 2.5f), Random.Range(-2.2f, 2.2f), 0);
                        warp_time = 0;
                    }
                    break;
                case 3:
                    if (time < 100)
                    {
                        transform.localPosition = enemy_end_pos - enemy_end_pos / 100 * time;
                    }
                    if (time == 100) { touch_count = 4; time = 0; walker_move.starter(); }
                    break;
                case 4:
                    if (time <= 202)
                    {
                        transform.localScale = new Vector3(size, size, 1);
                        transform.localPosition = new Vector3(
                            0 + animation_eat_end_pos.x / 202 * time,
                            0 + animation_eat_end_pos.y / 202 * time,
                            -6);
                        size -= 0.8f / 202;
                    }
                    if (time == 203) Destroy(gameObject);
                    break;
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (touch_count < 3)
                {
                    time = 0;
                    touch_count = 3;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 tapPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D collider = Physics2D.OverlapPoint(tapPoint);

                if (collider == _collider2D)
                {
                    if (time > 200)
                    {
                        switch (touch_count)
                        {
                            case 0:
                                touch_count++;
                                time = 0;
                                speed_regulation = 5;
                                HP_text.text = "2";
                                break;
                            case 1:
                                touch_count++;
                                time = 0;
                                HP_text.text = "1";
                                break;
                            case 2:
                                enemy_end_pos = transform.localPosition;
                                enemy_eat = true;
                                touch_count++;
                                time = 0;
                                HP_text.text = " ";
                                HP_text_slot.text = " ";
                                break;
                        }
                       
                    }

                    //Debug.Log(cout);
                }
            }
        }
    }

}
