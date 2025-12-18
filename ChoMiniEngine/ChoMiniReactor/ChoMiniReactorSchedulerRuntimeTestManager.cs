using System.Collections.Generic;
using UnityEngine;
using MessagePipe;
using Yoru.ChoMiniEngine;

public sealed class ChoMiniReactorSchedulerRuntimeTestManager : MonoBehaviour
{
    private ChoMiniReactorScheduler _scheduler;
    private ChoMiniScopeMessageContext _msg;

    void Start()
    {
        Debug.Log("=== Reactor Scheduler Runtime MOCK TEST START ===");

        // --------------------------------------------------
        // 1️⃣ MOCK NodeSources (태그 직접 부착)
        // --------------------------------------------------

        var nodeSources = new List<NodeSource>
        {
            new NodeSource(
                new object[] { new object() },
                "last-textNode"
            ),
            new NodeSource(
                new object[] { new object() }
            )
        };

        foreach (var src in nodeSources)
        {
            Debug.Log(
                $"[MOCK] NodeSource Has last-textNode = {src.HasTag("last-textNode")}"
            );
        }

        // --------------------------------------------------
        // 2️⃣ MOCK ReactorRules (Case 1, 2, 3)
        // --------------------------------------------------

        var rules = new List<ReactorRule>
        {
            // ================================
            // CASE 1
            // Provider + Tag
            // ================================
            new ReactorRule
            {
                ProviderType = typeof(ChoMiniGameObjectActivationProvider),
                IsLifetimeLoop = false,
                DoHook = () =>
                {
                    Debug.Log("MOCK CASE 1 DO (Provider + Tag)");
                },
                ScheduleConditions =
                {
                    new OnSequenceCompletedCondition()
                },
                NodeConditions =
                {
                    new NodeTagCondition("last-textNode")
                }
            },

            // ================================
            // CASE 2
            // Provider NULL + Tag
            // ================================
            new ReactorRule
            {
                ProviderType = null,
                DoHook = () =>
                {
                    Debug.Log("MOCK CASE 2 DO (NULL Provider + Tag)");
                },
                ScheduleConditions =
                {
                    new OnSequenceCompletedCondition()
                },
                NodeConditions =
                {
                    new NodeTagCondition("last-textNode")
                }
            },

            // ================================
            // CASE 3
            // Provider NULL + No Tag
            // ================================
            new ReactorRule
            {
                ProviderType = null,
                DoHook = () =>
                {
                    Debug.Log("MOCK CASE 3 DO (NULL Provider + No Tag)");
                },
                ScheduleConditions =
                {
                    new OnSequenceCompletedCondition()
                }
            }
        };

        // --------------------------------------------------
        // 3️⃣ Message Context
        // --------------------------------------------------

        _msg = new ChoMiniScopeMessageContext();

        // --------------------------------------------------
        // 4️⃣ Scheduler 생성
        // --------------------------------------------------

        _scheduler = new ChoMiniReactorScheduler(
            rules,
            nodeSources,
            _msg
        );

        // --------------------------------------------------
        // 5️⃣ SequenceComplete 트리거 강제 발행
        // --------------------------------------------------

        Debug.Log("=== Publish SequenceCompleted ===");
        _msg.SequenceCompletePublisher.Publish(new ChoMiniSOrchestratorPlaySequenceCompleteRequested());
    }

    private void OnDestroy()
    {
        _scheduler?.Dispose();
        _msg?.Dispose();
    }
}

// --------------------------------------------------
// MOCK Provider (내용 없음)
// --------------------------------------------------


