using MessagePipe;

namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// 초미니엔진 전역 입력(Command) 허브
    /// 외부(UI/키보드/스크립트)는 이 클래스를 통해 Play/Skip/Pause 등을 호출한다.
    /// 내부 엔진은 MessagePipe Publisher를 받아서 처리한다.
    /// </summary>
    public static class ChoMiniCommand
    {
        internal static IPublisher<ChoMiniCommandAdvanceRequested> SkipPub;


        // -------------------------------------------------------
        // 유저가 호출할 Public API
        // -------------------------------------------------------


        /// <summary>스토리 진행(다음 노드 재생 요청)</summary>
        public static void Advance()
        {
            SkipPub?.Publish(new ChoMiniCommandAdvanceRequested());
        }

        /// <summary>재생 일시정지</summary>
        public static void Pause()
        {

        }

        /// <summary>재생 재개</summary>
        public static void Resume()
        {

        }


        // -------------------------------------------------------
        // 엔진 초기화용 Setup 함수
        // FlowContainer.Build() 안에서 자동으로 호출된다.
        // -------------------------------------------------------
        internal static void Initialize(
        IPublisher<ChoMiniCommandAdvanceRequested> skip
        )
        {
            SkipPub = skip;
        }
    }

}