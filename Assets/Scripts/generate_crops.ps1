Add-Type -AssemblyName System.Drawing

$outDir = "C:\Users\Con voi cua Tai\My project\Assets\Sprite"
if (-not (Test-Path $outDir)) {
    New-Item -ItemType Directory -Path $outDir -Force | Out-Null
}

function Create-CropBitmap {
    param([int]$w = 128, [int]$h = 128)
    $bmp = New-Object System.Drawing.Bitmap($w, $h)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.Clear([System.Drawing.Color]::Transparent)
    return @($bmp, $g)
}

function Save-Crop {
    param($bmp, $g, [string]$filename)
    $g.Flush()
    $g.Dispose()
    $targetPath = Join-Path $outDir $filename
    $bmp.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    Write-Output "Saved: $filename"
}

# 1. Carrot
$b,$g = Create-CropBitmap
$leafBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 46, 178, 60))
$g.FillEllipse($leafBrush, 56, 12, 16, 32)
$g.FillEllipse($leafBrush, 40, 18, 20, 26)
$g.FillEllipse($leafBrush, 68, 18, 20, 26)
$leafBrush.Dispose()

$carrotBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 120, 10))
$cPoints = @(
    (New-Object System.Drawing.PointF(42, 42)),
    (New-Object System.Drawing.PointF(86, 42)),
    (New-Object System.Drawing.PointF(64, 118))
)
$g.FillPolygon($carrotBrush, $cPoints)
$g.FillEllipse($carrotBrush, 42, 36, 44, 14)
$carrotBrush.Dispose()

$stripePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 220, 85, 0), 3)
$g.DrawLine($stripePen, 52, 55, 68, 55)
$g.DrawLine($stripePen, 56, 72, 74, 72)
$g.DrawLine($stripePen, 60, 90, 70, 90)
$stripePen.Dispose()
Save-Crop $b $g "Crop_Carrot.png"

# 2. Tomato
$b,$g = Create-CropBitmap
$tomatoBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 235, 45, 35))
$g.FillEllipse($tomatoBrush, 28, 38, 72, 68)
$tomatoBrush.Dispose()

$hlBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(120, 255, 255, 255))
$g.FillEllipse($hlBrush, 40, 48, 18, 12)
$hlBrush.Dispose()

$stemBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 50, 160, 50))
$g.FillRectangle($stemBrush, 61, 20, 6, 20)
$g.FillEllipse($stemBrush, 46, 33, 16, 9)
$g.FillEllipse($stemBrush, 66, 33, 16, 9)
$g.FillEllipse($stemBrush, 56, 36, 16, 8)
$stemBrush.Dispose()
Save-Crop $b $g "Crop_Tomato.png"

# 3. Strawberry
$b,$g = Create-CropBitmap
$strawBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 225, 25, 55))
$sPoints = @(
    (New-Object System.Drawing.PointF(34, 46)),
    (New-Object System.Drawing.PointF(94, 46)),
    (New-Object System.Drawing.PointF(74, 108)),
    (New-Object System.Drawing.PointF(54, 108))
)
$g.FillClosedCurve($strawBrush, $sPoints)
$g.FillEllipse($strawBrush, 34, 38, 60, 24)
$strawBrush.Dispose()

$seedBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 240, 100))
$seeds = @(
    @(52,56), @(64,54), @(76,56),
    @(46,68), @(58,68), @(70,68), @(82,68),
    @(54,82), @(66,82), @(74,82),
    @(60,94), @(68,94)
)
foreach ($pt in $seeds) {
    $g.FillEllipse($seedBrush, $pt[0], $pt[1], 4, 6)
}
$seedBrush.Dispose()

$capBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 45, 170, 50))
$g.FillRectangle($capBrush, 62, 18, 5, 16)
$g.FillEllipse($capBrush, 36, 30, 24, 12)
$g.FillEllipse($capBrush, 68, 30, 24, 12)
$g.FillEllipse($capBrush, 52, 33, 24, 12)
$capBrush.Dispose()
Save-Crop $b $g "Crop_Strawberry.png"

# 4. Corn
$b,$g = Create-CropBitmap
$cornBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 200, 30))
$g.FillEllipse($cornBrush, 44, 25, 40, 75)
$cornBrush.Dispose()

$kPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 220, 160, 10), 2)
for ($y = 35; $y -lt 95; $y += 9) {
    $g.DrawLine($kPen, 48, $y, 80, $y)
}
for ($x = 52; $x -lt 80; $x += 9) {
    $g.DrawLine($kPen, $x, 30, $x, 95)
}
$kPen.Dispose()

$huskBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 80, 180, 50))
$huskL = @((New-Object System.Drawing.PointF(32, 95)), (New-Object System.Drawing.PointF(48, 55)), (New-Object System.Drawing.PointF(58, 112)))
$g.FillClosedCurve($huskBrush, $huskL)
$huskR = @((New-Object System.Drawing.PointF(96, 95)), (New-Object System.Drawing.PointF(80, 55)), (New-Object System.Drawing.PointF(70, 112)))
$g.FillClosedCurve($huskBrush, $huskR)
$g.FillRectangle($huskBrush, 58, 105, 12, 15)
$huskBrush.Dispose()
Save-Crop $b $g "Crop_Corn.png"

# 5. Wheat
$b,$g = Create-CropBitmap
$stalkPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 215, 170, 50), 4)
$g.DrawLine($stalkPen, 64, 115, 64, 30)
$stalkPen.Dispose()

$grainBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 240, 195, 60))
$grainPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 200, 150, 30), 2)
for ($y = 28; $y -lt 90; $y += 12) {
    # Left grain
    $g.FillEllipse($grainBrush, 44, $y, 18, 10)
    $g.DrawEllipse($grainPen, 44, $y, 18, 10)
    $g.DrawLine($grainPen, 44, $y + 5, 34, $y - 3)
    # Right grain
    $g.FillEllipse($grainBrush, 66, $y, 18, 10)
    $g.DrawEllipse($grainPen, 66, $y, 18, 10)
    $g.DrawLine($grainPen, 84, $y + 5, 94, $y - 3)
}
# Top grain
$g.FillEllipse($grainBrush, 57, 18, 14, 18)
$g.DrawLine($grainPen, 64, 18, 64, 8)
$grainBrush.Dispose()
$grainPen.Dispose()
Save-Crop $b $g "Crop_Wheat.png"

# 6. Sunflower
$b,$g = Create-CropBitmap
$stemBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 60, 150, 45))
$g.FillRectangle($stemBrush, 61, 60, 6, 60)
$g.FillEllipse($stemBrush, 38, 85, 24, 12)
$g.FillEllipse($stemBrush, 66, 75, 24, 12)
$stemBrush.Dispose()

# Yellow petals
$petalBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 205, 15))
$cx = 64; $cy = 52
for ($angle = 0; $angle -lt 360; $angle += 30) {
    $rad = $angle * [Math]::PI / 180.0
    $px = $cx + [Math]::Cos($rad) * 26 - 9
    $py = $cy + [Math]::Sin($rad) * 26 - 9
    $g.FillEllipse($petalBrush, [float]$px, [float]$py, 18, 18)
}
$petalBrush.Dispose()

# Brown center
$centerBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 100, 55, 25))
$g.FillEllipse($centerBrush, 44, 32, 40, 40)
$centerBrush.Dispose()
$seedPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 130, 75, 35), 2)
$g.DrawEllipse($seedPen, 50, 38, 28, 28)
$seedPen.Dispose()
Save-Crop $b $g "Crop_Sunflower.png"

# 7. Pumpkin
$b,$g = Create-CropBitmap
$pBody = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 240, 115, 15))
$g.FillEllipse($pBody, 22, 42, 84, 65)
$pBody.Dispose()

$pMid = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 135, 25))
$g.FillEllipse($pMid, 34, 40, 60, 68)
$pMid.Dispose()

$pCenter = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 255, 150, 35))
$g.FillEllipse($pCenter, 48, 40, 32, 68)
$pCenter.Dispose()

$stemBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 75, 130, 45))
$stemPts = @((New-Object System.Drawing.PointF(60, 43)), (New-Object System.Drawing.PointF(68, 43)), (New-Object System.Drawing.PointF(74, 24)), (New-Object System.Drawing.PointF(64, 23)))
$g.FillPolygon($stemBrush, $stemPts)
$stemBrush.Dispose()
Save-Crop $b $g "Crop_Pumpkin.png"

# 8. Watermelon
$b,$g = Create-CropBitmap
$wmBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 45, 140, 60))
$g.FillEllipse($wmBrush, 26, 36, 76, 72)
$wmBrush.Dispose()

$stripePen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 25, 85, 35), 6)
$stripePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$stripePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$g.DrawArc($stripePen, 34, 40, 24, 64, 100, 160)
$g.DrawArc($stripePen, 52, 38, 24, 68, 100, 160)
$g.DrawArc($stripePen, 70, 40, 24, 64, 100, 160)
$stripePen.Dispose()

$stemPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 80, 160, 50), 3)
$g.DrawArc($stemPen, 60, 20, 16, 18, 0, 240)
$stemPen.Dispose()
Save-Crop $b $g "Crop_Watermelon.png"

# 9. Eggplant
$b,$g = Create-CropBitmap
$epBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 95, 30, 125))
$epPts = @(
    (New-Object System.Drawing.PointF(52, 42)),
    (New-Object System.Drawing.PointF(76, 42)),
    (New-Object System.Drawing.PointF(92, 90)),
    (New-Object System.Drawing.PointF(72, 112)),
    (New-Object System.Drawing.PointF(44, 104)),
    (New-Object System.Drawing.PointF(38, 75))
)
$g.FillClosedCurve($epBrush, $epPts)
$epBrush.Dispose()

$hlBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(90, 255, 255, 255))
$g.FillEllipse($hlBrush, 45, 62, 12, 32)
$hlBrush.Dispose()

$capBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 70, 165, 60))
$g.FillRectangle($capBrush, 62, 18, 6, 16)
$g.FillEllipse($capBrush, 42, 34, 20, 12)
$g.FillEllipse($capBrush, 66, 34, 20, 12)
$g.FillEllipse($capBrush, 54, 38, 20, 12)
$capBrush.Dispose()
Save-Crop $b $g "Crop_Eggplant.png"

# 10. Radish
$b,$g = Create-CropBitmap
# Green leaves
$leafBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 60, 175, 70))
$g.FillEllipse($leafBrush, 56, 14, 16, 30)
$g.FillEllipse($leafBrush, 38, 22, 22, 24)
$g.FillEllipse($leafBrush, 68, 22, 22, 24)
$leafBrush.Dispose()

# Pink-red root body
$radishBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 225, 45, 105))
$g.FillEllipse($radishBrush, 38, 44, 52, 50)
$radishBrush.Dispose()

# White root tip
$tipBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 245, 235, 240))
$g.FillEllipse($tipBrush, 56, 84, 16, 16)
$tipPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 240, 230, 235), 3)
$g.DrawLine($tipPen, 64, 94, 64, 116)
$tipPen.Dispose()
$tipBrush.Dispose()
Save-Crop $b $g "Crop_Radish.png"

Write-Output "ALL 10 CROPS GENERATED SUCCESSFULLY!"
