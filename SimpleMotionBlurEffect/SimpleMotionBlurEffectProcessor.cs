using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;
using YukkuriMovieMaker.Plugin.Community.Effect.Video.CircularBlur;

namespace SimpleMotionBlurEffect
{
    internal class SimpleMotionBlurEffectProcessor(IGraphicsDevicesAndContext devices, SimpleMotionBlurEffect item) : VideoEffectProcessorBase(devices)
    {
        CircularBlurCustomEffect? circularBlurEffect;
        DirectionalBlur? directionalBlurEffect;
        RadialBlurCustomEffect? radialBlurEffect;

        bool isFirst = true;
        double circularBlur, directionalBlur, directionalBlurAngle, radialBlur;

        float rotation, zoom;
        Vector2 draw;
        int frame;

        public override DrawDescription Update(EffectDescription effectDescription)
        {
            var desc = effectDescription.DrawDescription;
            if (circularBlurEffect is null || directionalBlurEffect is null || radialBlurEffect is null)
                return desc;
            
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var circularBlurRate = item.CircularBlurRate.GetValue(frame, length, fps) / 100;
            var directionalBlurRate = item.DirectionalBlurRate.GetValue(frame, length, fps)/ 100;
            var radialBlurRate = item.RadialBlurRate.GetValue(frame, length, fps) / 100;

            float rotation, zoom;
            Vector2 draw;

            rotation = desc.Rotation.Z;
            draw = new Vector2(desc.Draw.X, desc.Draw.Y);
            zoom = (desc.Zoom.X + desc.Zoom.Y) * 50; //( / 2 * 100)

            float rotationOld, zoomOld;
            Vector2 drawOld;

            if (isFirst)
            {
                rotationOld = rotation;
                drawOld = draw;
                zoomOld = zoom;
            }
            else
            {
                rotationOld = this.rotation;
                drawOld = this.draw;
                zoomOld = this.zoom;
            }

            this.rotation = rotation;
            this.draw = draw;
            this.zoom = zoom;

            //プレビューを飛ばしたときDifferenceの値が大きくなって強いブラーがかかってしまう
            //フレームの差で割ることで緩和する
            var frameDifference = (this.frame == frame) ? 1 : Math.Abs(frame - this.frame);
            var rotationDifference = Math.Abs(rotation - rotationOld) / frameDifference;
            var drawDifference = Vector2.Distance(draw, drawOld) / frameDifference;
            var directionalBlurAngle = Math.Atan2(drawOld.Y - draw.Y, draw.X - drawOld.X) * 180 / Math.PI + rotation;
            var zoomDifference = Math.Abs(zoom - zoomOld) / frameDifference;
            
            this.frame = frame;

            var circularBlur = Math.Min(circularBlurRate * rotationDifference, 360);
            var directionalBlur = directionalBlurRate * drawDifference;
            var radialBlur = Math.Min(radialBlurRate * zoomDifference, 75);

            if (isFirst || this.circularBlur != circularBlur)
            {
                circularBlurEffect.Angle = (float)circularBlur;
                this.circularBlur = circularBlur;
            }
            if (isFirst || this.directionalBlur != directionalBlur)
            {
                directionalBlurEffect.StandardDeviation = (float)directionalBlur;
                this.directionalBlur = directionalBlur;
            }
            if (isFirst || this.directionalBlurAngle != directionalBlurAngle)
            {
                directionalBlurEffect.Angle = (float)directionalBlurAngle;
                this.directionalBlurAngle = directionalBlurAngle;
            }
            if (isFirst || this.radialBlur != radialBlur)
            {
                radialBlurEffect.Blur = (float)radialBlur;
                this.radialBlur = radialBlur;
                isFirst = false;
            }

            return effectDescription.DrawDescription;
        }

        protected override void setInput(ID2D1Image? input)
        {
            circularBlurEffect?.SetInput(0, input, true);
        }

        protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
        {
            circularBlurEffect = new CircularBlurCustomEffect(devices);
            if (!circularBlurEffect.IsEnabled)
            {
                circularBlurEffect.Dispose();
                circularBlurEffect = null;
                return null;
            }
            disposer.Collect(circularBlurEffect);

            directionalBlurEffect = new DirectionalBlur(devices.DeviceContext);
            disposer.Collect(directionalBlurEffect);

            radialBlurEffect = new RadialBlurCustomEffect(devices);
            if (!radialBlurEffect.IsEnabled)
            {
                radialBlurEffect.Dispose();
                radialBlurEffect = null;
                return null;
            }
            disposer.Collect(radialBlurEffect);

            using (var image = circularBlurEffect.Output)
            {
                directionalBlurEffect.SetInput(0, image, true);
            }
            using (var image = directionalBlurEffect.Output)
            {
                radialBlurEffect.SetInput(0, image, true);
            }

            var output = radialBlurEffect.Output;
            disposer.Collect(output);
            return output;
        }

        protected override void ClearEffectChain()
        {
            circularBlurEffect?.SetInput(0, null, true);
            directionalBlurEffect?.SetInput(0, null, true);
            radialBlurEffect?.SetInput(0, null, true);
        }
    }
}
