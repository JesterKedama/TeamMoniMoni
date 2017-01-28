using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameMainSceneChanger : MonoBehaviour
{

    [SerializeField]
    EnemyManager enemymanager;

    [SerializeField]
    StagingController staging;

    [SerializeField]
    EventsCreate eventcreate;

    void Start()
    {

    }


    void Update()
    {
        if (enemymanager.is_bosshit)
        {
            if (staging.flushStart())
            {
                SceneInfoManager.instance.is_scenechange = true;
                eventcreate.schoolEndReturnHouse();
                SceneManager.LoadScene("Hunting");
            }
        }

        if (enemymanager.is_enemy_hit)
        {
            if (staging.flushStart())
            {
                SceneInfoManager.instance.is_scenechange = true;
                eventcreate.stateSave();
                SceneManager.LoadScene("Hunting");
            }
        }

        if (eventcreate.is_schoolbossend)
        {
            SceneInfoManager.instance.is_scenechange = true;
            SceneManager.LoadScene("Ending");
        }
    }
}
