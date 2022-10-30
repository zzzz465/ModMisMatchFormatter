using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModMismatchFormatter
{
    public static class ModDiff
    {
        public static IEnumerable<(string, string)> DiffMods(List<string> lefts, List<string> rights)
        {
            var result = new List<(string, string)>();

            lefts.Reverse();
            rights.Reverse();

            string left = null;
            string right = null;

            if (lefts.Count > 0)
            {
                left = lefts.Pop();
            }
            if (rights.Count > 0)
            {
                right = rights.Pop();
            }

            while (left != null && right != null) // 여기서 요소가 있다는 것을 보장함, 대신 다 끝나고 1개 또는 0개가 남을 수 있음
            {
                if (left == right) // 동일할경우(그냥 추가하면 됨)
                {
                    result.Add((left, right));
                    left = lefts.PopOrNull();
                    right = rights.PopOrNull();
                    continue;
                }

                //만약 둘다 존재할경우
                int saveCurrentIndex = lefts.IndexOf(left);
                int activeCurrentIndex = rights.IndexOf(right);
                string SaveModInActiveModList = rights.AfterIndex(activeCurrentIndex).Where(item => item == left).FirstOrDefault(); // SaveModsToAdd.AfterIndex(saveCurrentIndex).Any(item => item.ModName == ActiveCurrent.ModName))
                string ActiveModInSaveModList = lefts.AfterIndex(saveCurrentIndex).Where(item => item == right).FirstOrDefault();
                if (SaveModInActiveModList != null && ActiveModInSaveModList != null)
                {
                    int SaveGapBetweenCurrentAndTarget = rights.IndexOf(SaveModInActiveModList) - rights.IndexOf(right);
                    int ActiveGapBetweenCurrentAndTarget = lefts.IndexOf(ActiveModInSaveModList) - lefts.IndexOf(left);

                    if (SaveGapBetweenCurrentAndTarget > ActiveGapBetweenCurrentAndTarget) // 만약 Save의 gap이 더 클 경우 Active을 먼저 더해주자
                    {
                        result.Add(("", right));
                        right = rights.PopOrNull();
                    }
                    else // Active의 Gap이 더 클경우 Save를 먼저 더해주자.
                    {
                        result.Add((left, ""));
                        left = lefts.PopOrNull();
                    }
                }
                //먼저 Active를 띄운다음 그다음 Save를 
                //만약 Save에만 존재할경우
                else if (SaveModInActiveModList == null)
                {
                    result.Add((left, ""));
                    left = lefts.PopOrNull();
                }
                //만약 Active에만 존재할경우
                else if (ActiveModInSaveModList == null)
                {
                    result.Add(("", right));
                    right = rights.PopOrNull();
                }
                else
                {
                    throw new Exception("Error in main Logic");
                }
            }

            while (left != null)
            {
                result.Add((left, ""));
                left = lefts.PopOrNull();
            }

            while (right != null)
            {
                result.Add(("", right));
                right = rights.PopOrNull();
            }

            return result;
        }
    }
}
