"""Draw the Thunderstore icon (256x256 PNG): ping ripples on the ground with a mouse cursor pointing at their centre.

Usage: python -I tools/make_icon.py thunderstore/icon.png
"""
import sys

from PIL import Image, ImageDraw, ImageFilter

S = 1024  # drawn at 4x, scaled down to 256 for smooth edges
OUT = 256

BG_TOP = (34, 26, 78)
BG_BOTTOM = (12, 10, 32)
GOLD = (255, 200, 72)
CURSOR_FILL = (255, 255, 255)
CURSOR_EDGE = (16, 14, 40)

# ripple centre = where the cursor points
CX, CY = 430, 470


def background():
    img = Image.new("RGBA", (S, S))
    d = ImageDraw.Draw(img)
    for y in range(S):
        t = y / (S - 1)
        d.line([(0, y), (S, y)], fill=tuple(round(a + (b - a) * t) for a, b in zip(BG_TOP, BG_BOTTOM)) + (255,))
    return img


def glow():
    layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    d.ellipse([CX - 170, CY - 110, CX + 170, CY + 110], fill=GOLD + (120,))
    return layer.filter(ImageFilter.GaussianBlur(70))


def ripples():
    layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)
    # flattened ellipses: rings lying on the ground, seen from the game's tilted top-down camera
    for rx, alpha, width in ((120, 255, 34), (250, 190, 28), (380, 110, 22)):
        ry = rx * 0.55
        d.ellipse([CX - rx, CY - ry, CX + rx, CY + ry], outline=GOLD + (alpha,), width=width)
    d.ellipse([CX - 34, CY - 20, CX + 34, CY + 20], fill=GOLD + (255,))
    return layer


def cursor():
    # classic arrow, tip at (0, 0), in 'cursor units'
    shape = [(0, 0), (0, 17), (4, 13.2), (7, 20), (9.6, 19), (6.6, 12.3), (12, 12.3)]
    scale = 24
    pts = [(CX + x * scale, CY + y * scale) for x, y in shape]
    layer = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    shadow = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    ImageDraw.Draw(shadow).polygon([(x + 18, y + 22) for x, y in pts], fill=(0, 0, 0, 150))
    layer.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(14)))
    d = ImageDraw.Draw(layer)
    d.polygon(pts, fill=CURSOR_FILL + (255,))
    d.line(pts + [pts[0]], fill=CURSOR_EDGE + (255,), width=26, joint="curve")
    return layer


def main(path):
    img = background()
    for layer in (glow(), ripples(), cursor()):
        img.alpha_composite(layer)
    img.convert("RGB").resize((OUT, OUT), Image.LANCZOS).save(path)
    print(f"wrote {path} ({OUT}x{OUT})")


if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else "thunderstore/icon.png")
