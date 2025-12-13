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

    }
}