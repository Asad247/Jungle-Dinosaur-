using UnityEngine;

public class WeaponLoader : MonoBehaviour
{

    [SerializeField] private GameObject[] weapons;
    void Awake()
    {
        int i = PlayerPrefs.GetInt("SelectedWeaponIndex");

        for (int j = 0; j < weapons.Length; j++)
        {
            weapons[j].SetActive(j == i);
        }


    }
}
