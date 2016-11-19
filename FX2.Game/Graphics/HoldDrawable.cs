// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;

namespace FX2.Game.Graphics
{
    public class HoldDrawable : Sprite
    {
        private Shader holdShader;

        protected override DrawNode CreateDrawNode() => new HoldDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);
            HoldDrawNode n = node as HoldDrawNode;
            n.HoldShader = holdShader;

            base.ApplyDrawNode(node);
        }
        
        [BackgroundDependencyLoader]
        private void Load(ShaderManager shaders)
        {
            if(holdShader == null)
                holdShader = shaders?.Load(@"sh_Position.vs", @"sh_Hold.fs");
        }
    }
}