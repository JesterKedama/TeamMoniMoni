using UnityEngine;
using System.Collections;
using System.Text;
using System;
using System.IO;

/// <summary>
/// イベントを(ブロックの番号)から読み込むクラス
/// </summary>
/// <remarks>
/// EventRepositoryからNumberを参照してEventManagerに格納する役割
/// </remarks>
public class EventLoader : MonoBehaviour
{
    [SerializeField]
    EventRepository eventrepository = null;

    [SerializeField]
    MapChipController mapcontroller = null;


    void Awake()
    {
        eventRegister();
    }

    public void eventRegister()
    {
        for (int y = 0; y < mapcontroller.chip_num_y; y++)
        {
            for (int x = 0; x < mapcontroller.chip_num_x; x++)
            {
                Block block = mapcontroller.blocks[(int)LayerController.Layer.EVENT] [y] [x].GetComponent<Block>();

                if (block.number != -1)
                {
                    mapcontroller.blocks[(int)LayerController.Layer.EVENT] [y] [x].GetComponent<Block>().
                        event_manager.addEvent(
                        eventrepository.getEvent(block.number),
                        eventrepository.getEventTriggerType(block.number));
                }
            }
        }
    }

    void Update()
    {

    }
}
