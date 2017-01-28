using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// プレイヤーを動かすボタンのスクリプト
/// </summary>
public class MoveButtonController : MonoBehaviour
{

    Vector2 mousepos;

    float buttonradius;

    Sprite[] buttonsprites;

    int buttonstatus;
    int current_buttonstatus;
    bool is_pressbutton; // ボタンが押されているかどうか

    Vector2 up;
    Vector2 down;
    Vector2 right;
    Vector2 left;

    void Start()
    {
        buttonsprites = Resources.LoadAll<Sprite>("Textures/UI/MoveButton");
        buttonstatus = 0;

        gameObject.GetComponent<Image>().sprite = System.Array.Find<Sprite>(
                                    buttonsprites, (sprite) => sprite.name.Equals(
                                        "MoveButton_" + buttonstatus.ToString()));

        buttonradius = gameObject.GetComponent<RectTransform>().sizeDelta.x / 2;

        is_pressbutton = false;

        up = new Vector2(0, 1.0f);
        down = new Vector2(0, -1.0f);
        right = new Vector2(1.0f, 0);
        left = new Vector2(-1.0f, 0);
    }


    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            gameObject.GetComponent<Image>().sprite = System.Array.Find<Sprite>(
                                buttonsprites, (sprite) => sprite.name.Equals(
                                    "MoveButton_0"));
            is_pressbutton = false;
        }

        var vec = getButtonPushVec();
        if (vec == Vector2.zero)
            buttonstatus = 0;
        else if (vec == up)
            buttonstatus = 1;
        else if (vec == down)
            buttonstatus = 2;
        else if (vec == right)
            buttonstatus = 3;
        else if (vec == left)
            buttonstatus = 4;

        if (current_buttonstatus == buttonstatus) return;
        current_buttonstatus = buttonstatus;

        gameObject.GetComponent<Image>().sprite = System.Array.Find<Sprite>(
                            buttonsprites, (sprite) => sprite.name.Equals(
                                "MoveButton_" + buttonstatus.ToString()));
    }

    public Vector2 getButtonPushVec()
    {
        if (!Input.GetMouseButton(0)) return Vector2.zero;
        return priortyVec();
    }



    Vector2 priortyVec()
    {
        int priority_vec = 0; // x 0 y 1
        var push_pos = pushValue();
        if (Mathf.Abs(push_pos.x) > Mathf.Abs(push_pos.y))
            priority_vec = 0;
        else
            priority_vec = 1;


        if (priority_vec == 0)
        {
            if (push_pos.x > 0.1f)
                return right;
            else if (push_pos.x < -0.1f)
                return left;
        }
        else if (priority_vec == 1)
        {
            if (push_pos.y > 0.1f)
                return up;
            else if (push_pos.y < -0.1f)
                return down;
        }

        return push_pos;
    }

    Vector2 pushValue()
    {
        mousepos = Input.mousePosition;
        Vector2 buttonpos = transform.position;
        Vector2 push_pos = Vector2.zero;
        if (is_pressbutton == false)
            if (!pointToCircle(buttonpos, buttonradius, mousepos)) return push_pos;
        is_pressbutton = true;
        Vector2 push_range = mousepos - buttonpos;

        float push_angle = Mathf.Atan2(push_range.x, push_range.y);

        float x = Mathf.Sin(push_angle) * buttonradius;
        float y = Mathf.Cos(push_angle) * buttonradius;

        push_pos = new Vector2(x, y);

        return push_pos;
    }


    // 点と円の判定(当たってたらtrue)
    bool pointToCircle(Vector2 circlepos, float radius, Vector2 pointpos)
    {
        float x = (pointpos.x - circlepos.x) * (pointpos.x - circlepos.x);
        float y = (pointpos.y - circlepos.y) * (pointpos.y - circlepos.y);

        return x + y <= radius * radius;
    }


}
