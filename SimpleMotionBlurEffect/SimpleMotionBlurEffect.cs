using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace SimpleMotionBlurEffect
{
    [VideoEffect("簡易モーションブラー", ["加工"], [], IsAviUtlSupported = false)]
    internal class SimpleMotionBlurEffect : VideoEffectBase
    {
        public override string Label => "簡易モーションブラー";

        [Display(GroupName = "簡易モーションブラー", Name = "回転ブラー", Description = "回転ブラー")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation CircularBlurRate { get; set; } = new Animation(0, 0, 1000);

        [Display(GroupName = "簡易モーションブラー", Name = "方向ブラー", Description = "方向ブラー")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation DirectionalBlurRate { get; set; } = new Animation(0, 0, 1000);

        [Display(GroupName = "簡易モーションブラー", Name = "放射ブラー", Description = "放射ブラー")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation RadialBlurRate { get; set; } = new Animation(0, 0, 1000);

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new SimpleMotionBlurEffectProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [CircularBlurRate, DirectionalBlurRate, RadialBlurRate];
    }
}
