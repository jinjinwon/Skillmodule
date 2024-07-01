using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class FloorHUD : MonoSingleton<FloorHUD>
{
    [SerializeField]
    private TextMeshProUGUI floorText;

    [Header("Stat View")]
    [SerializeField]
    private Image killFillImage;
    [SerializeField]
    private TextMeshProUGUI killValueText;

    [SerializeField]
    private Button action_Button;
    [SerializeField]
    private TextMeshProUGUI actionText;

    private Stage stage;


    public void Show(Stage stage)
    {
        ReleaseEvents();

        this.stage = stage;

        stage.onNextFloorGoalGauge += UpdateNextFloor;
        stage.onFloorChange += UpdateFloor;
        stage.onFloorGoalGaugeChange += UpdateKillView;
        stage.onBossSummon += UpdateBossSummon;

        action_Button.gameObject.SetActive(false);
    }

    private void ReleaseEvents()
    {
        if (!stage)
            return;

        stage.onNextFloorGoalGauge -= UpdateNextFloor;
        stage.onFloorGoalGaugeChange -= UpdateKillView;
        stage.onFloorChange -= UpdateFloor;
        stage.onBossSummon -= UpdateBossSummon;
    }

    private void UpdateNextFloor(Stage stage)
    {
        action_Button.gameObject.SetActive(true);

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/icons");

        Sprite targetSprite = null;
        foreach (var sprite in sprites)
        {
            if (sprite.name == "icons_18")
            {
                targetSprite = sprite;
                break;
            }
        }

        action_Button.image.sprite = targetSprite; // -> 임시용
        actionText.text = $"다음 층";

        action_Button.onClick.RemoveAllListeners();
        action_Button.onClick.AddListener(() => StageSystem.Instance.NextFloor());
    }

    private void UpdateBossSummon(Stage stage)
    {
        action_Button.gameObject.SetActive(true);

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/icons");

        Sprite targetSprite = null;
        foreach (var sprite in sprites)
        {
            if (sprite.name == "icons_27")
            {
                targetSprite = sprite;
                break;
            }
        }

        action_Button.image.sprite = targetSprite; // -> 임시용
        actionText.text = $"보스 소환";

        action_Button.onClick.RemoveAllListeners();
        action_Button.onClick.AddListener(()  => StageSystem.Instance.CreateBossNpc());
    }

    private void UpdateFloor(StageData stageData)
    {
        floorText.text = $"{stageData.floor} 층";
    }

    private void UpdateKillView(Stage stage, int currentKillCount, int maxKillCount)
    {
        killFillImage.fillAmount = (float)currentKillCount / maxKillCount;
        killValueText.text = $"{Mathf.RoundToInt(currentKillCount)} / {maxKillCount}";
    }
}
