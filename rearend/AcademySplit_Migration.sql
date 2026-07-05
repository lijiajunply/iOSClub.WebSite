-- 学院拆分数据迁移脚本
-- 背景：「信息与控制工程学院」拆分为「人工智能与机器人学院」和「计算机和信息工程学院」
-- 数据库：PostgreSQL
-- 表名：Students    字段：Academy (varchar(50))
--
-- ⚠️ 分流规则（已确认）：
--    学号前 2 位 >= '23'（即 2023 级及以后）→ 需要分流到两个新学院
--    学号前 2 位 <  '23'（即 2022 级及以前）→ 保留"信息与控制工程学院"不动
--
--    专业 → 新学院映射：
--      人工智能、自动化、机器人工程   → 人工智能与机器人学院
--      计算机科学与技术、通信工程     → 计算机和信息工程学院
--
-- ⚠️ 执行前必读：
--   1. 务必先备份数据库！
--      pg_dump -h <host> -U <user> -d <db> > backup_$(date +%Y%m%d).sql
--   2. 先运行「预检查」查看影响范围，确认无误后再执行「迁移」

-- 第 0 步：预检查（只读，不改数据）

-- 查看受影响的学生总数（学号 >= '23' 且 Academy = '信息与控制工程学院'）
SELECT COUNT(*) AS affected_count
FROM "Students"
WHERE "Academy" = '信息与控制工程学院'
  AND LEFT("UserId", 2) >= '23';

-- 查看受影响学生的明细（按班级分组，辅助判断拆分规则）
SELECT LEFT("UserId", 2) AS grade, "ClassName", COUNT(*) AS cnt
FROM "Students"
WHERE "Academy" = '信息与控制工程学院'
  AND LEFT("UserId", 2) >= '23'
GROUP BY LEFT("UserId", 2), "ClassName"
ORDER BY grade, cnt DESC;

-- 查看不受影响的学生（学号 < '23'，保留原学院）
SELECT COUNT(*) AS keep_count
FROM "Students"
WHERE "Academy" = '信息与控制工程学院'
  AND LEFT("UserId", 2) < '23';

-- 按专业（ClassName）自动拆分（已确认的规则）
-- 人工智能、自动化，机器人工程        → 人工智能与机器人学院
-- 计算机科学与技术、通信工程         → 计算机和信息工程学院

-- 0: 先看看哪些班级会被分到哪个学院（干跑，不写数据）
-- SELECT "ClassName", COUNT(*) AS cnt,
--        CASE
--            WHEN "ClassName" LIKE '%人工智能%' OR "ClassName" LIKE '%自动化%'
--                 OR "ClassName" LIKE '%机器人工程%'
--            THEN '→ 人工智能与机器人学院'
--            WHEN "ClassName" LIKE '%计算机科学与技术%' OR "ClassName" LIKE '%通信工程%'
--            THEN '→ 计算机和信息工程学院'
--            ELSE '→ 未匹配，保留不动'
--        END AS target_academy
-- FROM "Students"
-- WHERE "Academy" = '信息与控制工程学院'
--   AND LEFT("UserId", 2) >= '23'
-- GROUP BY "ClassName"
-- ORDER BY target_academy, cnt DESC;

-- 1: 划分到「人工智能与机器人学院」
-- UPDATE "Students"
-- SET "Academy" = '人工智能与机器人学院'
-- WHERE "Academy" = '信息与控制工程学院'
--   AND LEFT("UserId", 2) >= '23'
--   AND (
--     "ClassName" LIKE '%人工智能%'
--      OR "ClassName" LIKE '%自动化%'
--      OR "ClassName" LIKE '%机器人工程%'
--   );

-- 2: 划分到「计算机和信息工程学院」
-- UPDATE "Students"
-- SET "Academy" = '计算机和信息工程学院'
-- WHERE "Academy" = '信息与控制工程学院'
--   AND LEFT("UserId", 2) >= '23'
--   AND (
--     "ClassName" LIKE '%计算机科学与技术%'
--      OR "ClassName" LIKE '%通信工程%'
--   );

-- 3: 检查还有没有漏网之鱼（未匹配到任何关键词的）
-- SELECT "UserId", "UserName", "ClassName"
-- FROM "Students"
-- WHERE "Academy" = '信息与控制工程学院'
--   AND LEFT("UserId", 2) >= '23';
-- -- 如果有剩余记录，需人工处理


-- 迁移后验证

-- -- 确认 23 级以后不再有"信息与控制工程学院"
-- SELECT COUNT(*) AS should_be_zero
-- FROM "Students"
-- WHERE "Academy" = '信息与控制工程学院'
--   AND LEFT("UserId", 2) >= '23';

-- -- 确认 22 级及以前的都还在
-- SELECT COUNT(*) AS old_students_kept
-- FROM "Students"
-- WHERE "Academy" = '信息与控制工程学院'
--   AND LEFT("UserId", 2) < '23';

-- -- 查看迁移后的全学院分布
-- SELECT "Academy", COUNT(*) AS cnt
-- FROM "Students"
-- GROUP BY "Academy"
-- ORDER BY cnt DESC;

-- 回滚脚本（把分流过的改回旧值）
-- UPDATE "Students"
-- SET "Academy" = '信息与控制工程学院'
-- WHERE "Academy" IN ('人工智能与机器人学院', '计算机和信息工程学院')
--   AND LEFT("UserId", 2) >= '23';
