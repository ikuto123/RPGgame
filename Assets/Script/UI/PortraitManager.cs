using UnityEngine;
using UnityEngine.UI;

public class PortraitManager : MonoBehaviour
{
    public Image LeftImage, RightImage;
    
    //明るさの設定
    public Color ActiveColor = Color.white;
    public Color InactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    public void UpdatePortraits(Sprite leftSprite, Sprite rightSprite, PortraitPosition speakerSide)
    {
        // 1. 左の画像を更新（Noneでなければ差し替え）
        if (leftSprite != null)
        {
            LeftImage.sprite = leftSprite;
            LeftImage.gameObject.SetActive(true);
            LeftImage.preserveAspect = true; // アスペクト比を維持
        }

        // 2. 右の画像を更新（Noneでなければ差し替え）
        if (rightSprite != null)
        {
            RightImage.sprite = rightSprite;
            RightImage.gameObject.SetActive(true);
            RightImage.preserveAspect = true;
        }

        // 3. ハイライト（明るさ）と前後関係の調整
        HighlightSpeaker(speakerSide);
    }

    private void HighlightSpeaker(PortraitPosition side)
    {
        // 両方非表示なら何もしない
        if (!LeftImage.gameObject.activeSelf && !RightImage.gameObject.activeSelf) return;

        if (side == PortraitPosition.Left)
        {
            // 左を明るく、右を暗く
            LeftImage.color = ActiveColor;
            RightImage.color = InactiveColor;
        }
        else if (side == PortraitPosition.Right)
        {
            // 右を明るく、左を暗く
            LeftImage.color = InactiveColor;
            RightImage.color = ActiveColor;
        }
        else
        {
            // 両方暗く（または両方明るくしたい場合はActiveColorへ）
            LeftImage.color = InactiveColor;
            RightImage.color = InactiveColor;
        }
    }

    // 会話終了時などに画像を消す用
    public void ResetPortraits()
    {
        LeftImage.gameObject.SetActive(false);
        RightImage.gameObject.SetActive(false);
    }
}