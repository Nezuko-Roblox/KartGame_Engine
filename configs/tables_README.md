# Tables 表定义文件说明

## 概述

`tables` 目录包含了Luban配置表的表定义文件，用于描述如何从JSON数据文件中读取和组织配置数据。

## 文件结构

### 主表文件
- `main.xml` - 主表配置，引入所有子模块的表定义

### 游戏系统表定义
1. `item.xml` - 物品配置表定义
2. `egg.xml` - 蛋类配置表定义  
3. `boombox.xml` - 音箱配置表定义
4. `potion.xml` - 药水配置表定义
5. `mutation.xml` - 变异配置表定义
6. `island.xml` - 岛屿配置表定义
7. `size.xml` - 尺寸配置表定义
8. `hammer.xml` - 锤子配置表定义
9. `bundle.xml` - 捆绑包配置表定义
10. `expand.xml` - 扩展配置表定义
11. `foreverpack.xml` - 永久包配置表定义
12. `globalevent.xml` - 全局事件配置表定义
13. `portal.xml` - 传送门配置表定义
14. `tutorial.xml` - 教程配置表定义

## 表命名规范

所有表都遵循 `Tb{ModuleName}Config` 的命名规范：

- `TbItemConfig` - 物品配置表
- `TbEggConfig` - 蛋类配置表
- `TbBoomboxConfig` - 音箱配置表
- 等等...

## 数据源映射

每个表定义都指定了：
- `value` - 对应的数据类型定义（来自defines目录）
- `input` - 数据源文件（来自datas目录）
- `comment` - 表的说明

例如：
```xml
<table name="TbItemConfig" value="item.ItemConfig" input="item.json" comment="物品配置表"/>
```

## 使用方式

这些表定义文件将被Luban工具读取，用于：
1. 验证JSON数据文件的格式是否正确
2. 生成对应的代码文件
3. 创建运行时可用的配置表数据

## 与其他目录的关系

- **defines** → 定义数据结构类型
- **datas** → 提供实际的配置数据
- **tables** → 描述如何将数据组织成表
- **生成代码** → Luban根据这些定义生成最终的配置表代码
