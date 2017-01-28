using UnityEngine;
using System.Collections;

public class NightMareController : MonoBehaviour
{
    [SerializeField]
    MapChipController mapchip;

    public bool is_move;
    public float speed;
    int movecount;
    public Vector2 prev_cell;

    public bool can_capture = false;
    public int can_capture_count;

    [SerializeField]
    RuntimeAnimatorController week_animator;

    public void animationChange()
    {
        GetComponent<Animator>().runtimeAnimatorController = week_animator;
    }

    void Start()
    {
        is_move = false;
        movecount = 0;
        can_capture_count = 500;
        prev_cell = retCell();
    }

    void Update()
    {
        if (is_move == false) return;

        movecount++;
        if (movecount > can_capture_count)
            can_capture = true;
    }

    public Vector2 retCell()
    {
        var pos = transform.position;
        Vector2 cell_f = pos / mapchip.chip_size;

        Vector2 cell_i = new Vector2(Mathf.RoundToInt(cell_f.x), Mathf.RoundToInt(cell_f.y));
        cell_i.x = Mathf.Abs(cell_i.x);
        cell_i.y = Mathf.Abs(cell_i.y);

        return cell_i;
    }
}
