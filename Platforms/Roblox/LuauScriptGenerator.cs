using System.IO;

namespace AkpEngine.Platforms.Roblox
{
    /// <summary>
    /// Gera scripts Luau para lógica do jogo.
    /// </summary>
    public class LuauScriptGenerator
    {
        public void GenerateScripts(AkpProject project, RobloxExportSettings settings, string outputPath)
        {
            string scriptPath = Path.Combine(Path.GetDirectoryName(outputPath)!, "MainScript.lua");

            string luauCode = @"
local UIS = game:GetService('UserInputService')
local RS = game:GetService('RunService')
local platform = 'PC'
if UIS.TouchEnabled and not UIS.KeyboardEnabled then
    platform = UIS.GamepadEnabled and 'Console' or 'Mobile'
elseif UIS.GamepadEnabled then
    platform = 'Console'
end

local gui = game:GetService('Players').LocalPlayer:WaitForChild('PlayerGui'):WaitForChild('ScreenGui')
local scale = Instance.new('UIScale')
scale.Parent = gui
local function updateScale()
    local size = workspace.CurrentCamera.ViewportSize
    if platform == 'Mobile' and size.X <= 900 then
        scale.Scale = 0.75
    elseif platform == 'Tablet' and size.X > 900 then
        scale.Scale = 0.95
    else
        scale.Scale = 1.0
    end
end
workspace.CurrentCamera:GetPropertyChangedSignal('ViewportSize'):Connect(updateScale)
updateScale()

RS.RenderStepped:Connect(function(dt)
    -- Loop principal do jogo
end)
";

            File.WriteAllText(scriptPath, luauCode);
        }
    }
}
