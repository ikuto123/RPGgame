using UnityEngine;

public interface IInteractable
{
    void OnFocus();//注目したときのアクション
    void OnInteract(PlayerController player);//実行するアクション
    string ShowInteractionText(); //表示するテキスト
}