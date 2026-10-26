#!/usr/bin/env python3
"""Draw the Thunderstore icon (256x256): a campfire on a dark tile with a brass edge. Own art."""
import sys
from PIL import Image, ImageDraw, ImageFilter
S = 4; W = 256 * S                       # draw at 4x, downsample for clean edges
img = Image.new("RGBA", (W, W), (0, 0, 0, 0))
d = ImageDraw.Draw(img)
d.rounded_rectangle([8*S, 8*S, 248*S, 248*S], radius=36*S, fill=(24, 19, 16, 255), outline=(201, 160, 86, 255), width=6*S)
d.rounded_rectangle([20*S, 20*S, 236*S, 236*S], radius=28*S, outline=(214, 178, 110, 70), width=2*S)
glow = Image.new("RGBA", (W, W), (0, 0, 0, 0)); g = ImageDraw.Draw(glow)
g.ellipse([48*S, 60*S, 208*S, 220*S], fill=(240, 150, 60, 110)); glow = glow.filter(ImageFilter.GaussianBlur(30*S))
img = Image.alpha_composite(img, glow)
fire = Image.new("RGBA", (W, W), (0, 0, 0, 0)); f = ImageDraw.Draw(fire)
# crossed logs
f.rounded_rectangle([58*S, 182*S, 198*S, 204*S], radius=10*S, fill=(110, 72, 42, 255), outline=(52, 34, 20, 255), width=3*S)
log = Image.new("RGBA", (W, W), (0, 0, 0, 0)); l = ImageDraw.Draw(log)
l.rounded_rectangle([58*S, 182*S, 198*S, 204*S], radius=10*S, fill=(132, 88, 50, 255), outline=(52, 34, 20, 255), width=3*S)
fire = Image.alpha_composite(fire, log.rotate(18, center=(128*S, 193*S), resample=Image.BICUBIC))
fire = Image.alpha_composite(fire, log.rotate(-18, center=(128*S, 193*S), resample=Image.BICUBIC))
# flames: outer gold, middle orange, inner pale
def flame(c, x0, x1, top, base, col):
    f2 = ImageDraw.Draw(fire)
    f2.polygon([(x0*S, base*S), (c*S, top*S), (x1*S, base*S)], fill=col)
    f2.ellipse([x0*S, (base-34)*S, x1*S, (base+8)*S], fill=col)
flame(128, 86, 170, 40, 172, (232, 150, 52, 255))
flame(128, 98, 158, 70, 176, (246, 190, 84, 255))
flame(128, 110, 146, 104, 178, (252, 232, 170, 255))
img = Image.alpha_composite(img, fire)
img.resize((256, 256), Image.LANCZOS).save(sys.argv[1])
