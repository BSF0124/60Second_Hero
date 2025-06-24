using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 파티 리스트 화면 이동 및 버튼 제어 처리
/// </summary>
public class CameraMove : MonoBehaviour
{
    public Transform objects;
    public Button leftButton;
    public Button rightButton;

    public int maxPosition;
    public int currentPosition = 0;

    private void Start()
    {
        leftButton.onClick.AddListener(MovetoLeft);
        rightButton.onClick.AddListener(MovetoRight);
        SetMaxPosition();
    }

    /// <summary>
    /// 현재 플레이어 수에 따라 아동 한계 및 초기 위치 설정
    /// </summary>
    public void SetMaxPosition()
    {
        float duration = GetMoveDuration();
        objects.DOMove(Vector3.zero, duration);
        
        int playerCount = DataManager.instance.gameData.players.Count;
        maxPosition = playerCount >= 2 ? (playerCount - 2) / 8 + 1 : 0;

        leftButton.gameObject.SetActive(maxPosition > 0);
        rightButton.gameObject.SetActive(false);
        currentPosition = 0;
    }

    /// <summary>
    /// 왼쪽으로 한 칸 이동
    /// </summary>
    public void MovetoLeft()
    {
        if (currentPosition >= maxPosition) return;
        
        AudioManager.instance.PlaySfx(0);
        float duration = GetMoveDuration();

        currentPosition++;
        Vector3 targetPos = new(GameConstants.moveUnit * currentPosition, 0f, 0f);
        objects.DOMove(targetPos, duration);
        
        leftButton.gameObject.SetActive(currentPosition < maxPosition);
        rightButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 오른쪽으로 한 칸 이동
    /// </summary>
    public void MovetoRight()
    {
        if (currentPosition <= 0) return;
        
        AudioManager.instance.PlaySfx(0);
        float duration = GetMoveDuration();

        currentPosition--;
        Vector3 targetPos = new(GameConstants.moveUnit * currentPosition, 0f, 0f);
        objects.DOMove(targetPos, duration);

        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(currentPosition > 0);
    }
    
    /// <summary>
    /// 초기 위치로 이동
    /// </summary>
    public void MovetoInit()
    {
        if (currentPosition <= 0) return;

        currentPosition = 0;
        float duration = GetMoveDuration();
        objects.DOMove(Vector3.zero, duration);

        leftButton.gameObject.SetActive(maxPosition > 0);
        rightButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 이동 속도 설정(배속 어빌리티 여부 반영)
    /// </summary>
    private float GetMoveDuration()
    {
        return DataManager.instance.gameData.abilities[6].isActivate ? 0.25f : 0.5f;
    }
}
