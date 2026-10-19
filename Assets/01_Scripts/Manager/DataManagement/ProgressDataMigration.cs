using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 게임 업데이트 시 저장 데이터 포맷 변경에 따른 마이그레이션 로직을 담당
/// 새 컨텐츠 추가 시 MigrateToVersionN 메서드를 추가해 주세요
/// </summary>
public static class ProgressDataMigration
{
    // 데이터 버전 관리 - 새 컨텐츠 추가 시 증가 (1 -> 2 -> 3 ...)
    public const int CURRENT_DATA_VERSION = 1;

    /// <summary>
    /// 데이터 마이그레이션 메인 메서드
    /// LoadProgress()에서 호출됨
    /// </summary>
    public static void MigrateData(ProgressSaveData data, int fromVersion, int toVersion)
    {
        if (fromVersion >= toVersion)
            return;

        for (int v = fromVersion + 1; v <= toVersion; v++)
        {
            Debug.Log($"[ProgressDataMigration] 데이터 마이그레이션 시작: v{v - 1} → v{v}");

            switch (v)
            {
                case 2:
                    MigrateToVersion2(data);
                    break;
                case 3:
                    MigrateToVersion3(data);
                    break;
                // 추가 버전 시 여기에 case 추가
                default:
                    Debug.LogWarning($"[ProgressDataMigration] 버전 {v}에 대한 마이그레이션 메서드가 없습니다.");
                    break;
            }
        }
    }

    /// <summary>
    /// v1 → v2 마이그레이션
    /// 새로 추가되는 필드를 여기서 초기화
    /// </summary>
    private static void MigrateToVersion2(ProgressSaveData data)
    {
        // 새로운 내용 !@#$
        // 예: if (data.newField == null) data.newField = new();
        Debug.Log("[ProgressDataMigration] v2 마이그레이션 완료");
    }

    /// <summary>
    /// v2 → v3 마이그레이션
    /// </summary>
    private static void MigrateToVersion3(ProgressSaveData data)
    {
        // 새로운 내용 !@#$
        Debug.Log("[ProgressDataMigration] v3 마이그레이션 완료");
    }
    
    // 위와 같은 방식으로 ProgressDataManager에 저장할 데이터 포맷이 변경될 때마다 메서드를 통한 마이그레이션 로직 적용.
}
