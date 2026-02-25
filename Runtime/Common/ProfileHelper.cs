#define ENABLE_PROFILE_DUMP
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Common
{
    public static class ProfileHelper
    {
        // 存储每个key对应的计时器
        private static readonly Dictionary<string, Stopwatch> Stopwatches = new Dictionary<string, Stopwatch>();

        /// <summary>
        /// 开始性能检测计时
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        public static void StartProfile(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            // 如果已经存在，先停止并移除旧的
            if (Stopwatches.ContainsKey(key))
            {
                Stopwatches[key].Stop();
                Stopwatches.Remove(key);
            }

            // 创建新的计时器并开始计时
            var stopwatch = Stopwatch.StartNew();
            Stopwatches[key] = stopwatch;
        }

        /// <summary>
        /// 暂停性能检测计时（保留已过去的时间，可在之后恢复）
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        public static void PauseProfile(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            
            if (!Stopwatches.TryGetValue(key, out var stopwatch))
                return;
            
            // 如果已经在运行，则暂停（Stop()会保留Elapsed时间）
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
            }
        }

        /// <summary>
        /// 恢复性能检测计时（从上次暂停的时间点继续累加）
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        public static void ResumeProfile(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            
            if (!Stopwatches.TryGetValue(key, out var stopwatch))
                return;
            
            // 如果已停止，则恢复（Start()会从当前Elapsed时间继续累加）
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }
        }

        /// <summary>
        /// 获取当前耗时（不停止计时）
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        /// <returns>当前耗时（毫秒），如果key不存在则返回-1</returns>
        public static int StepProfile(string key)
        {
            if (string.IsNullOrEmpty(key))
                return -1;

            if (!Stopwatches.TryGetValue(key, out var stopwatch))
                return -1;

            // 返回当前已耗时的毫秒数
            return (int)stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// 停止性能检测计时并返回总耗时
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        /// <returns>总耗时（毫秒），如果key不存在则返回-1</returns>
        public static int StopProfile(string key)
        {
            if (string.IsNullOrEmpty(key))
                return -1;

            if (!Stopwatches.TryGetValue(key, out var stopwatch))
                return -1;

            // 停止计时
            stopwatch.Stop();
            
            // 获取总耗时
            int elapsedMs = (int)stopwatch.ElapsedMilliseconds;
            
            // 移除计时器
            Stopwatches.Remove(key);
            
            return elapsedMs;
        }

        /// <summary>
        /// 清理所有性能检测计时器
        /// </summary>
        public static void ClearAll()
        {
            foreach (var stopwatch in Stopwatches.Values)
            {
                stopwatch.Stop();
            }
            Stopwatches.Clear();
        }

        /// <summary>
        /// 检查指定key是否正在计时
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        /// <returns>是否正在计时（存在且正在运行）</returns>
        public static bool IsProfiling(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            
            return Stopwatches.TryGetValue(key, out var stopwatch) && stopwatch.IsRunning;
        }

        /// <summary>
        /// 检查指定key是否存在（无论是否正在运行）
        /// </summary>
        /// <param name="key">性能检测点的唯一标识</param>
        /// <returns>是否存在</returns>
        public static bool HasProfile(string key)
        {
            return !string.IsNullOrEmpty(key) && Stopwatches.ContainsKey(key);
        }

        public static void StartProfileDump(string key, string message = "")
        {
#if ENABLE_PROFILE_DUMP
            StartProfile(key);
            UnityEngine.Debug.Log($"ProfileDump: Start {key} <{message}>, {Time.time}s");
#endif
        }

        public static void StepProfileDump(string key, string message = "")
        {
#if ENABLE_PROFILE_DUMP
            if (!IsProfiling(key))
            {
                StartProfileDump(key, message);
                return;
            }
            var elapsed = StepProfile(key);
            UnityEngine.Debug.Log($"ProfileDump: Step {key} <{message}>, {Time.time}s, elapsed: {elapsed}ms");
#endif
        }

        public static void EndProfileDump(string key, string message = "")
        {
#if ENABLE_PROFILE_DUMP
            var elapsed = StopProfile(key);
            UnityEngine.Debug.Log($"ProfileDump: End {key} <{message}>, {Time.time}s, elapsed: {elapsed}ms");
#endif
        }
    }
}