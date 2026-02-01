using System;
using System.Collections.Generic;

namespace Stella.Util
{
    public static class LZ77
    {
        private const int minLength = 3;

        public static byte[] Decompress(byte[] data)
        {
            List<byte> res = new List<byte>();
            int pos = 0;
            int state = 0;

            while (pos < data.Length)
            {
                state >>= 1;
                if (state <= 1)
                    state = data[pos++] | 0x100;

                if ((state & 1) != 0)
                {
                    res.Add(data[pos++]);
                }
                else
                {
                    byte byte1 = data[pos++];
                    byte byte2 = data[pos++];

                    int length = (byte2 & 0xf) + minLength;
                    int distance = (byte1 << 4) | (byte2 >> 4);

                    if (distance == 0)
                        break;

                    int resPos = res.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        int o = resPos - distance + i;
                        res.Add((o < 0) ? (byte)0 : res[o]);
                    }
                }
            }

            return res.ToArray();
        }

        private struct Match
        {
            public int distance;
            public int length;
        }

        // [최적화] 원본 로직과 100% 동일한 결과를 보장하는 고속 탐색
        private static unsafe Match FindLongestMatch(byte* pData, int dataLen, int offset, int windowSize, int lookAhead, int minLength)
        {
            int bestLen = minLength - 1;
            int bestDist = -1;

            int maxMatch = (lookAhead < dataLen - offset) ? lookAhead : dataLen - offset;

            if (maxMatch < minLength)
            {
                return new Match { distance = -1, length = -1 };
            }

            // offset이 windowSize보다 작을 때(파일 시작 부분)를 위한 처리
            int limitDist = (offset < windowSize) ? offset : windowSize;
            byte* pCurr = pData + offset;

            // 1. [Fast Path] 참조 범위가 배열 내부인 경우 (99%의 케이스)
            // 포인터 연산으로 빠르게 비교합니다.
            for (int dist = 1; dist <= limitDist; dist++)
            {
                byte* pRef = pCurr - dist;

                // 첫 바이트 불일치 시 빠른 스킵
                if (*pCurr != *pRef) continue;

                int len = 1;
                // LZ77 압축 시 현재 버퍼 내 비교는 memcmp와 동일하게 순차 비교 가능
                // (Overlap된 경우에도 원본 데이터가 이미 버퍼에 있으므로 유효함)
                while (len < maxMatch && *(pCurr + len) == *(pRef + len))
                {
                    len++;
                }

                // "더 긴" 길이를 찾았을 때만 업데이트 (길이가 같으면 더 짧은 거리인 현재 dist 유지 -> 원본 우선순위 준수)
                if (len > bestLen)
                {
                    bestLen = len;
                    bestDist = dist;
                    if (bestLen == maxMatch) break;
                }
            }

            // 2. [Slow Path] 참조 범위가 0보다 작은 경우 (GV(음수) -> 0 처리 필요)
            // 파일의 맨 앞부분(windowSize 이전)에서만 발생
            if (limitDist < windowSize)
            {
                for (int dist = limitDist + 1; dist <= windowSize; dist++)
                {
                    int currentLen = 0;
                    int compBase = offset - dist; // 음수

                    for (int i = 0; i < maxMatch; i++)
                    {
                        // 원본 코드의 RepeatingSequencesEqual 로직 구현:
                        // 가상의 스트림에서 (offset - dist) 위치부터 읽되, 음수 인덱스는 0으로 처리
                        // 그리고 패턴은 dist 주기로 반복됨 (LZ77 특성)

                        int refIdx = compBase + (i % dist);
                        byte valRef = (refIdx < 0) ? (byte)0 : pData[refIdx];

                        if (pData[offset + i] != valRef) break;
                        currentLen++;
                    }

                    if (currentLen > bestLen)
                    {
                        bestLen = currentLen;
                        bestDist = dist;
                        if (bestLen == maxMatch) break;
                    }
                }
            }

            return new Match
            {
                distance = bestDist,
                length = (bestDist != -1) ? bestLen : -1
            };
        }

        public static unsafe byte[] Compress(byte[] data, int windowSize = 256, int lookAhead = 0xf + minLength)
        {
            if (lookAhead < minLength || lookAhead > 0xf + minLength)
                throw new ArgumentException("lookAhead out of range", "lookAhead");
            if (windowSize < lookAhead)
                throw new ArgumentException("windowSize needs to be larger than lookAhead", "windowSize");

            int len = data.Length;
            // 결과 버퍼 넉넉히 할당
            byte[] result = new byte[len + (len / 8) + 64];

            int resOffset = 1;
            int resStateOffset = 0;
            int resStateShift = 0;
            int offset = 0;

            // [중요] 첫 번째 플래그 바이트 초기화
            result[0] = 0;

            fixed (byte* pData = data)
            {
                while (offset < len)
                {
                    Match match = FindLongestMatch(pData, len, offset, windowSize, lookAhead, minLength);

                    if (match.length >= minLength && match.distance > 0)
                    {
                        int binLength = match.length - minLength;

#if DEBUG
                        if (binLength > 0xf || match.distance > 0xfff || match.length > len - offset)
                            throw new Exception("INTERNAL ERROR: found match is invalid!");
#endif
                        // 버퍼 오버플로우 방지 및 확장
                        if (resOffset + 2 >= result.Length)
                            Array.Resize(ref result, result.Length * 2);

                        result[resOffset++] = (byte)(match.distance >> 4);
                        result[resOffset++] = (byte)(((match.distance & 0xf) << 4) | binLength);
                        resStateShift += 1;
                        offset += match.length;
                    }
                    else
                    {
                        if (resOffset + 1 >= result.Length)
                            Array.Resize(ref result, result.Length * 2);

                        // Literal 플래그 설정
                        result[resStateOffset] |= (byte)(1 << resStateShift++);
                        result[resOffset++] = data[offset++];
                    }

                    // 플래그 바이트가 꽉 찼을 때
                    if (resStateShift >= 8)
                    {
                        resStateShift = 0;
                        resStateOffset = resOffset++;

                        // [핵심 수정] 새로 할당된 플래그 바이트 위치를 반드시 0으로 초기화해야 함.
                        // 배열 재사용 시 이전 데이터가 남아있을 수 있기 때문입니다.
                        if (resOffset <= result.Length) // 범위 체크
                        {
                            if (resStateOffset < result.Length)
                                result[resStateOffset] = 0;
                        }

                        // 확장 필요 시 수행 (다음 루프를 위해)
                        if (resOffset >= result.Length)
                            Array.Resize(ref result, result.Length * 2);
                    }
                }
            }

            // [핵심 수정] 원본 코드의 +2 로직 유지 (EOF 마커 0x00 0x00 보장)
            // cstream은 이 부분이 없으면 "데이터가 중간에 끊겼다"고 판단합니다.
            Array.Resize(ref result, resOffset + 2);

            return result;
        }
    }
}