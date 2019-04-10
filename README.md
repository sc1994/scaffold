# scaffold

>基本N层，数据库到Model，使用EF Core

## 基本功能

- 数据库结构查看
- 生成对应数据库表属性的基本N层代码
- 使用EF对数据库进行操作

---

## 使用

- 虽然是个web程序，但是因为涉及到文件读写，故只能进行本地部署才能使用

```sh
git clone https://github.com/sc1994/scaffold.git
cd scaffold/scaffold
dotnet run
```

- 默认本地机器已经安装了dotnet core 2.2 开发环境
- 访问`localhost:5000`
- 被生成代码的项目须有满足一下条件
  - `Model`层以`xxx.Models`项目命名方式，`xxx`指待你的项目名称
  - `Database`层以`xxx.Database`项目命名方式，同上
  - `Service`层以`xxx.Services`项目命名方式，同上

## todo
 
- [ ] 初始化项目结构（用处不大）
- [ ] 支持指定文件夹生成，而不是使用默认