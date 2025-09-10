// CharacterProtection.server.ts
// 服务器端脚本：保护玩家角色不死亡

import { Players, RunService } from "@rbxts/services";

// 保护角色不死亡的函数
function protectCharacter(character: Model): void {
    const humanoid = character.WaitForChild("Humanoid", 10) as Humanoid | undefined;
    if (!humanoid) {
        return;
    }
    
    // 取消角色所有部件的锚固
    for (const part of character.GetDescendants()) {
        if (part.IsA("BasePart")) {
            part.Anchored = true;
        }
    }
    
    // 设置无限生命值
    humanoid.MaxHealth = math.huge;
    humanoid.Health = math.huge;
    
    // 监听生命值变化
    const healthConnection = humanoid.HealthChanged.Connect((health: number) => {
        if (health < humanoid.MaxHealth) {
            humanoid.Health = humanoid.MaxHealth;
        }
    });
    
    // 监听死亡事件
    const diedConnection = humanoid.Died.Connect(() => {
        // 立即复活
        humanoid.Health = humanoid.MaxHealth;
        humanoid.ChangeState(Enum.HumanoidStateType.Running);
    });
    
    // 防止掉出地图
    let fallCheckConnection: RBXScriptConnection | undefined;
    fallCheckConnection = RunService.Heartbeat.Connect(() => {
        if (character.PrimaryPart) {
            const position = character.PrimaryPart.Position;
            // 如果角色掉到Y=-500以下，传送回出生点
            if (position.Y < -500) {
                const spawnLocation = game.Workspace.FindFirstChild("SpawnLocation") as SpawnLocation | undefined;
                if (spawnLocation) {
                    character.SetPrimaryPartCFrame(spawnLocation.CFrame.add(new Vector3(0, 5, 0)));
                } else {
                    // 如果没有出生点，传送到原点上方
                    character.SetPrimaryPartCFrame(new CFrame(0, 50, 0));
                }
                humanoid.Health = humanoid.MaxHealth;
            }
        }
    });
    
    // 清理连接
    humanoid.AncestryChanged.Connect(() => {
        if (!humanoid.Parent) {
            if (healthConnection) {
                healthConnection.Disconnect();
            }
            if (diedConnection) {
                diedConnection.Disconnect();
            }
            if (fallCheckConnection) {
                fallCheckConnection.Disconnect();
            }
        }
    });
}

// 当玩家加入时
Players.PlayerAdded.Connect((player: Player) => {
    // 当角色生成时
    player.CharacterAdded.Connect((character: Model) => {
        protectCharacter(character);
        print("CharacterProtection: 保护", player.Name, "的角色");
    });
    
    // 注释掉CharacterAutoLoads，因为这个属性不存在
    // player.CharacterAutoLoads = false
    
    // 如果角色已经存在
    if (player.Character) {
        protectCharacter(player.Character);
    }
});

// 对于已经在游戏中的玩家
for (const player of Players.GetPlayers()) {
    // player.CharacterAutoLoads = false  // 注释掉，属性不存在
    if (player.Character) {
        protectCharacter(player.Character);
    }
    
    player.CharacterAdded.Connect((character: Model) => {
        protectCharacter(character);
    });
}

print("CharacterProtection: 服务器端角色保护已启动");

// 返回空对象以满足 require
export default {};
