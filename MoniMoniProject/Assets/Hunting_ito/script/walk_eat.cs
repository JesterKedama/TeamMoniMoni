﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class walk_eat : MonoBehaviour
{
    public bool start_judge;

    private Image image_eat;
    [SerializeField]
    GameObject slider;
    Slide slider_ani;
    //float end_size = 0.08f;
    private Vector3 animation_end_pos = new Vector3(-4, -1.1f, -5);
    //private Vector3 animation_eat_end_pos = new Vector3(2.5f, -0.16f, -6);
    private Vector3 animation_pos;
    private Vector3 animation_walk;
    //private Vector3 animation_end_walk;
    private float size;
    private int animation_time;
    public Sprite eat_1;
    public Sprite eat_2;
    public Sprite eat_3;
    public int eat_count;

    // Use this for initialization
    void Start()
    {
        slider_ani = slider.GetComponent<Slide>();
        size = 0.0200002f;
        animation_pos = transform.localPosition;
        animation_walk = new Vector3(
            animation_pos.x - animation_end_pos.x,
            animation_pos.y - animation_end_pos.y,
            0);
        image_eat = GetComponent<Image>();
        eat_count = 0;

        //if (eater == false)
        //{
        //    size = 0.01f;
        //    animation_pos = transform.localPosition;
        //    animation_end_walk = new Vector3(
        //   animation_pos.x - animation_eat_end_pos.x,
        //   animation_pos.y - animation_eat_end_pos.y,
        //   0);
        //}

    }
    //x 9.0引くy 1.5引く
    //
    // Update is called once per frame
    void Update()
    {
        if (start_judge == true)
        {
            animation_time++;
            transform.localScale = new Vector3(size, size, 1);
            transform.localPosition = animation_pos;
            //
            //0.02    
            if (animation_time <= 100)
            {
                animation_pos -= animation_walk / 500;
                size += 0.00012f;
                //0.012
            }//0.032
            if (animation_time > 100 && animation_time <= 200)
            {
                animation_pos -= animation_walk * 3 / 1000;
                size += 0.00018f;
                //0.018
            }//0.05
            if (animation_time > 200 && animation_time <= 300)
            {
                animation_pos -= animation_walk / 200;
                size += 0.0003f;
                //0.03
            }//0.08
            if (animation_time == 190) eat_count = 1;
            if (animation_time == 205) eat_count = 2;
            if (animation_time == 301)
            {

                slider_ani.SlideStart();
                Destroy(gameObject);
            }
            switch (eat_count)
            {
                case 0:
                    image_eat.sprite = eat_1;
                    break;
                case 1:
                    image_eat.sprite = eat_2;
                    break;
                case 2:
                    image_eat.sprite = eat_3;
                    break;

            }
        }
    }
    public void starter() {
        if (start_judge == false) start_judge = true;
    }
    //if (eater == false)
    //{
    //    if (animation_time <= 200)
    //    {
    //        animation_pos = new Vector3(
    //            -animation_end_walk.x * animation_time / 200,
    //            -animation_end_walk.y * animation_time / 200,
    //            animation_pos.z);
    //        size -= size / 200;
    //    }
    //    if (animation_time == 201) Destroy(gameObject);
    //}

}
