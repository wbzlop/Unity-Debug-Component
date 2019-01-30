# Unity-Debug-Component
为方便测试同事，写的测试工具。可查看log,fps,内存等信息。

![priview](https://github.com/wbzlop/Unity-Debug-Component/blob/master/img.png)

## 使用
```c#
gameObj.AddComponent<DebugTool> ();
```

## 增加测试按钮
```c#
DebugTool.AddButton ("死亡",delegate() {
				DelegateCenter.onGameOver(DeadMode.NoMove);
});
```

## 增加toggle
```c#
DebugTool.AddToogle("无敌", (bool obj) =>
{

});
```
