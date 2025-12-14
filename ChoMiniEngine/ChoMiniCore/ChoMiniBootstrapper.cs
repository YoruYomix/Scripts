using UnityEngine;
using Yoru.ChoMiniEngine.Utility;


namespace Yoru.ChoMiniEngine
{
    /// <summary>
    /// 🌈 초미니 엔진의 전역 부팅 시스템.
    /// - 📡 MessagePipe 글로벌 초기화
    /// - 🎮 ChoMiniCommand 연결
    /// - 🔒 중복 부팅 방지
    /// </summary>
    public static class ChoMiniBootstrapper
    {
        private static bool _booted = false;

        private static ChoMiniCommandContext _commandContext;

        public static ChoMiniCommandContext CommandContext
            => _commandContext;

        /// <summary>
        /// 🌟 게임 시작 시 반드시 한 번 호출해야 하는 함수!
        /// ExampleGameBoot.Start() 등에서 호출해주세요.
        /// </summary>
        public static void Boot()
        {
            if (_booted)
            {
                Debug.Log("⚠️ [ChoMiniEngine] Boot() — 이미 부팅된 상태예요. (중복 호출 무시)");
                return;
            }

            try
            {
                _booted = true;

                Debug.Log("🚀 [ChoMiniEngine] Boot() — 초미니 세계를 여는 중... ✨");


                // ---------------------------------------------------------
                // 1) 📡 MessagePipe 글로벌 설정
                // ---------------------------------------------------------
                _commandContext = new ChoMiniCommandContext();



                Debug.Log("📡 [ChoMiniEngine] MessagePipe 준비 완료! (초미니 이벤트 버스 구축!)");


                // ---------------------------------------------------------
                // 2) 🎮 ChoMiniCommand 내부 Publisher 연결
                // ---------------------------------------------------------
                ChoMiniCommand.Initialize(
                    _commandContext.SkipPublisher
                );


                Debug.Log("🎮 [ChoMiniEngine] 초미니 커맨드 시스템 연결 완료! (Play/Skip/Pause 등 작동!)");


                // ---------------------------------------------------------
                // 3) 🌱 확장 가능 영역
                // ---------------------------------------------------------
                // - 글로벌 리소스 로더
                // - 게임 공용 설정
                // - 글로벌 EventBus 추가 등

                Debug.Log("🌈 [ChoMiniEngine] 부팅 완료! 이제 초미니 세계가 작동해요 😊✨");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"💥 [ChoMiniEngine] Boot 실패! 오류 메시지: {ex.Message}");
                throw;
            }
        }
    }
}