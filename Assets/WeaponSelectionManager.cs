using UnityEngine;
using TMPro;
using DG.Tweening;

[System.Serializable]
public class WeaponData
{
    public string weaponID;
    public GameObject weaponModel;
    public int price;
}

public class WeaponSelectionManager : MonoBehaviour
{
    public static WeaponSelectionManager Instance { get; private set; }

    [Header("Weapon Configuration")]
    public WeaponData[] weapons;

    [Header("UI References")]
    public GameObject lockImage;
    public GameObject playButton;
    public GameObject unlockButton;
    public TextMeshProUGUI priceText;

    private int viewingIndex = 0;

    private const string SELECTED_WEAPON_INDEX_KEY = "SelectedWeaponIndex";
    private const string UNLOCKED_PREFIX = "WeaponUnlocked_";
    private const string COINS_KEY = "coins";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeSystem();
    }

    private void InitializeSystem()
    {
        if (weapons == null || weapons.Length == 0) return;

        // Flaw Check: Prevent empty IDs from colliding and corrupting save data
        for (int i = 0; i < weapons.Length; i++)
        {
            if (string.IsNullOrEmpty(weapons[i].weaponID))
                Debug.LogError($"WeaponSelectionManager: Weapon at index {i} has no weaponID. Unlocks will overwrite.");
        }

        // Force first weapon to be unlocked and execute immediate save
        PlayerPrefs.SetInt(UNLOCKED_PREFIX + weapons[0].weaponID, 1);
        PlayerPrefs.Save();

        viewingIndex = PlayerPrefs.GetInt(SELECTED_WEAPON_INDEX_KEY, 0);

        // Safety: Reset to 0 if saved index exceeds array length or isn't unlocked
        if (viewingIndex >= weapons.Length || !IsWeaponUnlocked(viewingIndex))
        {
            viewingIndex = 0;
            SaveSelectedWeapon(0);
        }

        UpdateDisplay();
    }

    public void NextWeapon()
    {
        if (weapons.Length == 0) return;
        viewingIndex = (viewingIndex + 1) % weapons.Length;
        UpdateDisplay();
    }

    public void PreviousWeapon()
    {
        if (weapons.Length == 0) return;
        viewingIndex = (viewingIndex - 1 + weapons.Length) % weapons.Length;
        UpdateDisplay();
    }

    [Header("DOTween Settings")]
    public float animationDuration = 0.4f;
    public Ease bounceEase = Ease.OutBack;

    private void UpdateDisplay()
    {
        if (weapons == null || weapons.Length == 0) return;

        bool isUnlocked = IsWeaponUnlocked(viewingIndex);

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i].weaponModel != null)
            {
                bool isCurrent = (i == viewingIndex);

                // If it's the weapon we are switching TO
                if (isCurrent && !weapons[i].weaponModel.activeSelf)
                {
                    weapons[i].weaponModel.SetActive(true);

                    // Kill any old scale animations and reset scale
                    weapons[i].weaponModel.transform.DOKill();
                    weapons[i].weaponModel.transform.localScale = Vector3.zero;

                    // The Pop/Bounce Animation
                    weapons[i].weaponModel.transform.DOScale(Vector3.one, animationDuration)
                        .SetEase(bounceEase)
                        .SetUpdate(true); // Works even if game is paused
                }
                else if (!isCurrent)
                {
                    weapons[i].weaponModel.SetActive(false);
                }
            }
        }

        // --- Keep the rest of your UI logic below ---
        if (lockImage != null) lockImage.SetActive(!isUnlocked);
        if (playButton != null) playButton.SetActive(isUnlocked);
        if (unlockButton != null) unlockButton.SetActive(!isUnlocked);

        if (priceText != null)
        {
            priceText.gameObject.SetActive(!isUnlocked);
            if (!isUnlocked) priceText.text = weapons[viewingIndex].price.ToString();
        }

        if (isUnlocked)
        {
            SaveSelectedWeapon(viewingIndex);
        }
    }

    public void AttemptBuyWeapon()
    {
        int currentCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        int cost = weapons[viewingIndex].price;

        if (!IsWeaponUnlocked(viewingIndex) && currentCoins >= cost)
        {
            // Deduct coins
            PlayerPrefs.SetInt(COINS_KEY, currentCoins - cost);

            // Unlock weapon and save immediately
            PlayerPrefs.SetInt(UNLOCKED_PREFIX + weapons[viewingIndex].weaponID, 1);
            PlayerPrefs.Save();

            // Update display forces the UI to change and triggers the auto-equip logic
            UpdateDisplay();
        }
    }

    public WeaponData GetSelectedWeaponData()
    {
        int index = GetSelectedWeapon();
        // Prevent IndexOutOfRange if the array size was reduced in the editor
        if (index >= 0 && index < weapons.Length)
        {
            return weapons[index];
        }
        return weapons[0];
    }

    /// <summary>
    /// Returns the integer index of the currently selected weapon.
    /// </summary>
    public int GetSelectedWeapon()
    {
        return PlayerPrefs.GetInt(SELECTED_WEAPON_INDEX_KEY, 0);
    }

    public void SelectWeapon(int index)
    {
        if (index >= 0 && index < weapons.Length)
        {
            viewingIndex = index;
            UpdateDisplay();
        }
    }

    public void UnlockAll()
    {
        foreach (var weapon in weapons)
        {
            PlayerPrefs.SetInt(UNLOCKED_PREFIX + weapon.weaponID, 1);
        }
        PlayerPrefs.Save();
        UpdateDisplay();
    }

    public void ReassignUI(GameObject lockImg, GameObject playBtn, GameObject unlockBtn, TextMeshProUGUI priceTxt)
    {
        lockImage = lockImg;
        playButton = playBtn;
        unlockButton = unlockBtn;
        priceText = priceTxt;
        UpdateDisplay();
    }

    private bool IsWeaponUnlocked(int index)
    {
        if (index == 0) return true;
        return PlayerPrefs.GetInt(UNLOCKED_PREFIX + weapons[index].weaponID, 0) == 1;
    }

    private void SaveSelectedWeapon(int index)
    {
        PlayerPrefs.SetInt(SELECTED_WEAPON_INDEX_KEY, index);
        PlayerPrefs.Save();
    }
}