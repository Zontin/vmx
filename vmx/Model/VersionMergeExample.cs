using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vmx.Model
{
    public class VersionMergeExample
    {
        public static void DoMerge(string OfileName, string AfileName, string BfileName, string CfileName)
        {
            List<string> O = new List<string>(File.ReadAllLines(OfileName));
            List<string> A = new List<string>(File.ReadAllLines(AfileName));
            List<string> B = new List<string>(File.ReadAllLines(BfileName));
            List<string> C = new List<string>();

            int[] OA = CreateLastCommonSubsequency(O, A);
            int[] AO = CreateLastCommonSubsequency(A, O);
            int[] OB = CreateLastCommonSubsequency(O, B);
            int[] BO = CreateLastCommonSubsequency(B, O);


            int LA = 0;
            int LB = 0;
            int LO = 0;

            do
            {
                int i = FindDiffPoint(LO, LA, LB, OA, AO, OB, BO);

                if (i == 1)
                {
                    // (a)
                    bool isAchanged, isBchanged;
                    int a, b;

                    int o = FindJoinPoint(LO, LA, LB, OA, AO, OB, BO, out a, out b, out isAchanged, out isBchanged);

                    if (o > LO)
                    {
                        if (isAchanged && isBchanged)
                        {
                            C.Add("// Конфликт");
                            C.Add("// A =============");
                            C.AddRange(A.Slice(LA, a - 1));
                            C.Add("// B =============");
                            C.AddRange(B.Slice(LB, b - 1));
                            C.Add("// * =============");
                        }
                        else if (isAchanged)
                        {
                            C.AddRange(A.Slice(LA, a - 1));
                        }
                        else if (isBchanged)
                        {
                            C.AddRange(B.Slice(LB, b - 1));
                        }
                        LO = o - 1;
                        LA = a - 1;
                        LB = b - 1;
                    }
                    else
                    {
                        if (isAchanged && isBchanged)
                        {
                            C.Add("// Конфликт");
                            C.Add("// A =============");
                            C.AddRange(A.Slice(LA, A.Count));
                            C.Add("// B =============");
                            C.AddRange(B.Slice(LB, B.Count));
                            C.Add("// * =============");
                        }
                        else if (isAchanged)
                        {
                            C.AddRange(A.Slice(LA, A.Count));
                        }
                        else if (isBchanged)
                        {
                            C.AddRange(B.Slice(LB, B.Count));
                        }
                        break;
                    }
                }
                else if (i > 1)
                {
                    // (b)
                    C.AddRange(O.Slice(LO, LO + i - 1));
                    LO = LO + i - 1;
                    LA = LA + i - 1;
                    LB = LB + i - 1;
                }
                else
                {
                    C.AddRange(O.Slice(LO, O.Count));
                    break;
                }
            } while (true);

            File.WriteAllLines(CfileName, C.ToArray());

        }

        private static int FindJoinPoint(int LO, int LA, int LB, int[] OA, int[] AO, int[] OB, int[] BO, out int a, out int b, out bool isAchanged, out bool isBchanged)
        {
            isAchanged = false;
            isBchanged = false;
            a = 0;
            b = 0;

            for (int ro = LO + 1; ro < OA.Length; ro++)
            {
                a = OA[ro];
                b = OB[ro];

                if (a == 0 || AO[a] != ro)
                {
                    isAchanged = true;
                    a = 0;
                }

                if (b == 0 || BO[b] != ro)
                {
                    isBchanged = true;
                    b = 0;
                }

                if (a != 0 && b != 0)
                {

                    for (int i = LA + 1; i < a; i++)
                    {
                        isAchanged = isAchanged || AO[i] == 0;
                    }

                    for (int i = LB + 1; i < b; i++)
                    {
                        isBchanged = isBchanged || BO[i] == 0;
                    }

                    return ro;
                }
            }
            return 0;
        }

        private static int FindDiffPoint(int LO, int LA, int LB, int[] OA, int[] AO, int[] OB, int[] BO)
        {
            for (int i = 1; LO + i < OA.Length && LA + i < AO.Length && LB + i < BO.Length; i++)
            {
                if (OA[LO + i] == 0 || OB[LO + i] == 0 || AO[LA + i] == 0 || BO[LB + i] == 0)
                {
                    return i;
                }
            }
            return 0;
        }

        private static int[] CreateLastCommonSubsequency(List<string> O, List<string> A)
        {
            int m = O.Count;
            int n = A.Count;

            char[] ignore = new char[] { ' ', '\t' };

            List<LineHash> V = new List<LineHash>(n);
            V.AddRange(A.Select((x, i) => new LineHash() { Number = i + 1, Line = x.DiffHash(ignore) }));
            V.Sort((x, y) =>
            {
                int r = x.Line.CompareTo(y.Line);
                if (r == 0)
                {
                    r = x.Number.CompareTo(y.Number);
                }
                return r;
            });

            EquivalenceClass[] E = new EquivalenceClass[n + 1];
            E[0] = new EquivalenceClass() { Number = 0, Last = true };
            for (int j = 1; j < n; j++)
            {
                E[j] = new EquivalenceClass() { Number = V[j - 1].Number, Last = V[j - 1].Line.CompareTo(V[j].Line) != 0 };
            }
            E[n] = new EquivalenceClass() { Number = V[n - 1].Number, Last = true };

            int[] P = new int[m];
            for (int i = 0; i < m; i++)
            {
                string key = O[i].DiffHash(ignore);
                int j = V.FindIndex(x => x.Line.CompareTo(key) == 0);
                if (j >= 0 && E[j].Last)
                {
                    P[i] = j + 1;
                }
                else
                {
                    P[i] = 0;
                }
            }

            List<CandidateRec> K = new List<CandidateRec>();
            K.Add(new CandidateRec() { a = 0, b = 0, prev = null });
            K.Add(new CandidateRec() { a = m + 1, b = n + 1, prev = null });
            int k = 0;
            for (int i = 0; i < m; i++)
            {
                if (P[i] != 0)
                {
                    MergeCandidate(K, ref k, i + 1, E, P[i]);
                }
            }

            int[] J = new int[m + 1];
            for (int i = 0; i <= m; i++)
            {
                J[i] = 0;
            }

            CandidateRec c = K[k];
            do
            {
                J[c.a] = c.b;
                c = c.prev;
            } while (c != null);

            for (int i = 0; i < m; i++)
            {
                int j = J[i + 1];
                if (j != 0 && O[i].DiffHash(ignore).CompareTo(A[j - 1].DiffHash(ignore)) != 0)
                {
                    J[i + 1] = 0;
                }
            }

            return J;

        }

        private static void MergeCandidate(List<CandidateRec> K, ref int k, int i, EquivalenceClass[] E, int p)
        {
            int r = 0;
            CandidateRec c = K[0];
            do
            {
                int j = E[p].Number;
                int s = -1;
                for (int l = r; l <= k; l++)
                {
                    if (K[l].b < j && K[l + 1].b > j)
                    {
                        s = l;
                        break;
                    }
                }
                if (s >= 0)
                {
                    if (K[s + 1].b > j)
                    {
                        K[r] = c;
                        r = s + 1;
                        c = new CandidateRec() { a = i, b = j, prev = K[s] };
                    }
                    if (s == k)
                    {
                        K.Add(K[k + 1]);
                        k = k + 1;
                        break;
                    }
                }
                if (E[p].Last)
                {
                    break;
                }
                else
                {
                    p = p + 1;
                }
            } while (true);
            K[r] = c;
        }
    }

    [DebuggerDisplay("{Number} : {Line}")]
    class LineHash
    {
        public int Number { get; set; }
        public string Line { get; set; }
    }

    [DebuggerDisplay("{Number} : {Last}")]
    class EquivalenceClass
    {
        public int Number { get; set; }
        public bool Last { get; set; }
    }

    [DebuggerDisplay("{a} - {b} : {prev}")]
    class CandidateRec
    {
        public int a { get; set; }
        public int b { get; set; }
        public CandidateRec prev { get; set; }
    }

    public static class Extensions
    {
        public static string DiffHash(this string source, char[] ignore)
        {
            return source.Trim(ignore);
        }

        public static IEnumerable<string> Slice(this List<string> source, int start, int end)
        {
            return source.Skip(start).Take(end - start);
        }
    }
}
