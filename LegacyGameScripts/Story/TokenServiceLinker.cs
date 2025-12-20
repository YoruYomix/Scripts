using System.Threading;
using UnityEngine;
using UnityEngine.UI;


// 순수 정적(Static) 클래스로, 유니티 엔진에 독립적입니다.
public static class TokenService
{
    private static CancellationTokenSource _cts;

    // 클래스가 처음 접근될 때 한 번만 실행됩니다.
    static TokenService()
    {
        // 초기화 시 토큰 소스 생성
        _cts = new CancellationTokenSource();
    }

    // ------------------- 퍼블릭 인터페이스 -------------------

    public static CancellationToken GetCurrentToken()
    {
        return _cts.Token;
    }

    // 새로운 작업 텀을 시작하고 이전 토큰을 취소합니다.
    public static void CancelRequest()
    {
        Debug.Log("TokenService: 새 토큰 발행");
        _cts?.Cancel();
        _cts?.Dispose();

        _cts = new CancellationTokenSource();
    }

    // 취소 요청
    public static void RequestCancel()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            Debug.Log("TokenService: 취소 요청 발동.");
        }
    }

    // 자원 해제 (중개자 OnDestroy에서 호출됨)
    public static void Dispose()
    {
        _cts?.Dispose();
        Debug.Log("TokenService: 자원 해제 완료.");
    }
}

public class TokenServiceLinker : MonoBehaviour
{
    // 1. 싱글톤 인스턴스 변수 선언
    public static TokenServiceLinker Instance { get; private set; }
    //  인스펙터에 연결할 버튼 (메인 진행 버튼)
    [SerializeField] private Button actionButton;


    private void Awake()
    {
        // 2. 이미 살아있는 인스턴스가 있는지 확인
        if (Instance != null)
        {
            // 이미 존재한다면, 새로 생성된 자신(this)은 파괴하고 종료합니다.
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        // 3. 현재 인스턴스를 유일한 인스턴스로 설정
        Instance = this;

        // 4. 파괴되지 않도록 설정 (이 작업은 한 번만 유효함)
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (actionButton != null)
        {
            // 버튼 이벤트를 정적 클래스의 메서드에 연결 (중개 역할)
            // actionButton.onClick.AddListener(TokenService.StartNewAction);
            // 만약 취소 버튼도 있다면: cancelButton.onClick.AddListener(TokenService.RequestCancel);
        }
        else
        {
            Debug.LogError("TokenServiceLinker에 Action Button이 연결되지 않았습니다.");
        }
    }

    private void OnDestroy()
    {
        // GameObject가 파괴될 때 정적 서비스의 자원도 함께 해제합니다.
        TokenService.Dispose();
    }
}