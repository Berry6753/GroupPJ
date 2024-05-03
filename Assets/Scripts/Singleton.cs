using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T :MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            //인스턴스가 null이면
            if (instance == null)
            {
                //만약 씬에 이미 해당타입의 인스턴스가 있으면
                instance = (T)FindObjectOfType(typeof(T));

                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<T>();

                    DontDestroyOnLoad(singletonObject);//씬이 변경되더라도 해당오브젝트를 없애지않음
                }
            }

            return instance;
        }
    }
}
