Imports MovingSprite
Imports System.Threading
'MINITECH's Arkanoid Game Code
Public Class Form1
#Region "Constants"
    Const brWidth As Integer = 70
    Const brHeight As Integer = 30
    Const baSize As Integer = 22
    Const pWidth As Integer = 150
    Const pHeight As Integer = 10
#End Region
#Region "Game Variables"
    Dim ball As MovingSprite.MovingSprite
    Dim paddle_p As Point
    Dim bricks(3) As BitArray '4 rows, 9 cols (use last 9 bits)
    Dim gameprogress As Int16 = 0
    Dim score As Integer = 0
#End Region
    Dim mustclose As Boolean = False
#Region "Bitmaps"
    Dim ball_bmp, brick_bmp As Bitmap
#End Region
    Dim tGameEngine, tGameCycle As Thread
    Private Declare Function GetAsyncKeyState Lib "user32" Alias "GetAsyncKeyState" (ByVal vKey As Long) As Integer
    Delegate Sub DNoParams()
#Region "Event Handlers"

    Private Sub Form1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        If gameprogress = 1 Then gameprogress = 2
        Me.Invalidate()
    End Sub
    Private Sub Form_Closing()
        mustclose = True
        tGameEngine.Join()
        tGameCycle.Join()
        End
    End Sub
    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Randomize()
        tGameEngine = New Thread(AddressOf Game_Engine)
        tGameCycle = New Thread(AddressOf Game_Cycle)
        tGameEngine.Start() : tGameCycle.Start()
        'Fullscreen
        Me.Location = New Point(0, 0)
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.None
        Me.ClientSize = Screen.GetBounds(New Point(1, 1)).Size
        '----------Bitmap init.----------
        Try
            ball_bmp = Bitmap.FromFile("bitmaps/ball.bmp")
            brick_bmp = Bitmap.FromFile("bitmaps/brick.bmp")
        Catch ex As IO.FileNotFoundException
            MsgBox("File " & ex.Message & " not found.", MsgBoxStyle.Exclamation, "File not found.")
            End
        End Try
        'Make magenta transparent (#FF00FF)
        ball_bmp.MakeTransparent(Color.FromArgb(&HFFFF00FF))
        brick_bmp.MakeTransparent(Color.FromArgb(&HFFFF00FF))
        '----------End Bitmap init. section----------


        '----------Initialization----------
        ball = New MovingSprite.MovingSprite(ball_bmp, New Point((Me.ClientSize.Width / 2) - (baSize / 2), 41 + baSize), 10, -10)
        Dim i As Integer = 0, j As Integer = 0
        While i < bricks.Length
            bricks(i) = New BitArray(11)
            While j < bricks(i).Count
                bricks(i)(j) = True
                j += 1
            End While
            j = 0
            i += 1
        End While
        paddle_p = New Point(0, 0) 'Will be reset by "NewGame()" call.
        '----------End main init. section----------
        NewGame()
        gameprogress = 1
        Me.DoubleBuffered = True
    End Sub
    Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
        Dim objGraphics As Graphics = e.Graphics
        Dim centered As New StringFormat()
        centered.Alignment = StringAlignment.Center
        centered.LineAlignment = StringAlignment.Center
        Dim fTitle As New Font("Arial", 52)
        Dim fExit As New Font("Courier", 44)
        Select Case gameprogress
            Case 0
                Exit Sub
            Case 1
                objGraphics.DrawString("Arkanoid", fTitle, Brushes.White, Me.ClientRectangle, centered)
            Case 2
                objGraphics.DrawImage(ball_bmp, ball.Location.X, ball.Location.Y, baSize, baSize)
                objGraphics.FillRectangle(Brushes.Yellow, New Rectangle(paddle_p, New Size(pWidth, pHeight)))
                DrawBricks(objGraphics)
            Case 3
                objGraphics.DrawImage(ball_bmp, ball.Location.X, ball.Location.Y, baSize, baSize)
                objGraphics.FillRectangle(Brushes.Yellow, New Rectangle(paddle_p, New Size(pWidth, pHeight)))
                DrawBricks(objGraphics)
                objGraphics.DrawString("Quit? (Y/N)", fExit, Brushes.Lime, Me.ClientRectangle, centered)
        End Select
        MyBase.OnPaint(e)
    End Sub
#End Region
#Region "Subs & Functions"
    Private Sub DrawBricks(ByRef g As Graphics)
        Dim i As Integer = 0
        Dim j As Integer = 0
        While i < bricks.Length
            While j < bricks(i).Length
                If bricks(i)(j) Then
                    g.DrawImage(brick_bmp, j * brWidth, i * brHeight, brWidth, brHeight)
                End If
                j += 1
            End While
            j = 0
            i += 1
        End While
    End Sub
    Private Sub NewGame()
        Dim i As Integer = 0
        Dim j As Integer = 0
        While i < bricks.Length
            While j < bricks(i).Length
                bricks(i)(j) = True
                j += 1
            End While
            j = 0
            i += 1
        End While
        i = 0
        score = 0
        ball.Location = New Point((Me.ClientSize.Width / 2) - (baSize / 2), Me.ClientSize.Height - (41 + baSize))
        ball.Speed = 12
        ball.Angle = -12
        paddle_p = New Point((Me.ClientSize.Width / 2) - (pWidth / 2), Me.ClientSize.Height - 40)
        Me.Invalidate()
    End Sub
    Private Sub MoveBall()
        ball.Move()
        If ball.X < 0 Then
            ball.X = 0
            ball.Speed *= -1
        ElseIf (ball.X + baSize) > Me.ClientSize.Width Then
            ball.X = Me.ClientSize.Width - baSize
            ball.Speed *= -1
        End If
        If ball.Y < 0 Then
            ball.Y = 0
            ball.Angle *= -1
        ElseIf (ball.Y + baSize) > Me.ClientSize.Height Then
            gameprogress = 1
            MsgBox("You lost!", MsgBoxStyle.Information, "Arkanoid")
            NewGame()
            gameprogress = 2
        End If
        If ball.DoesIntersect(New Rectangle(paddle_p, New Size(pWidth, pHeight))) Then
            ball.Y = Me.ClientSize.Height - (41 + baSize)
            Dim bxCenter, pxCenter As Integer
            bxCenter = ball.X + (baSize / 2)
            pxCenter = paddle_p.X + (pWidth / 2)
            ball.Speed += CInt(Math.Floor((bxCenter - pxCenter) / 25))
            ball.Angle *= -1
        End If
        'Now, for the bricks
        Dim i As Integer = 0, j As Integer = 0
        Dim baRect, brRect As Rectangle, baPts As Point()
        baRect = New Rectangle(ball.X, ball.Y, baSize, baSize)
        baPts = ShFunctionality.GetRectPoints(baRect)
        While i < bricks.Length
            While j < bricks(i).Count
                If bricks(i)(j) Then
                    brRect = New Rectangle(j * brWidth, i * brHeight, brWidth, brHeight)
                    Dim l As List(Of Integer)
                    l = ShFunctionality.PointsInside(baPts, brRect)
                    If l.Contains(ShFunctionality.RectPtsIndex.TopLeft) And l.Contains(ShFunctionality.RectPtsIndex.TopRight) Then
                        'From bottom
                        ball.Y = brRect.Y + brRect.Height
                        ball.Angle *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.BottomLeft) And l.Contains(ShFunctionality.RectPtsIndex.BottomRight) Then
                        'From top
                        ball.Y = brRect.Y - baSize
                        ball.Angle *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.BottomLeft) And l.Contains(ShFunctionality.RectPtsIndex.TopLeft) Then
                        'From right
                        ball.X = brRect.X + brRect.Width
                        ball.Speed *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.BottomRight) And l.Contains(ShFunctionality.RectPtsIndex.TopRight) Then
                        'From left
                        ball.X = brRect.X - baSize
                        ball.Speed *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.BottomLeft) Then
                        'From top-right corner
                        ball.X = brRect.X + brRect.Width
                        ball.Speed *= -1
                        ball.Y = brRect.Y - baSize
                        ball.Angle *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.BottomRight) Then
                        'From top-left corner
                        ball.X = brRect.X - baSize
                        ball.Speed *= -1
                        ball.Y = brRect.Y - baSize
                        ball.Angle *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.TopLeft) Then
                        'From bottom-right corner
                        ball.X = brRect.X + brRect.Width
                        ball.Speed *= -1
                        ball.Y = brRect.Y + brRect.Height
                        ball.Angle *= -1
                        bricks(i)(j) = False
                    ElseIf l.Contains(ShFunctionality.RectPtsIndex.TopRight) Then
                        'From bottom-left corner
                        ball.X = brRect.X - baSize
                        ball.Speed *= -1
                        ball.Y = brRect.Y + brRect.Height
                        ball.Angle *= -1
                        bricks(i)(j) = False
                    End If
                    CheckWin()
                End If
                j += 1
            End While
            j = 0
            i += 1
        End While
    End Sub
    Private Sub CheckWin()
        Dim haswon As Boolean = True
        Dim i As Integer = 0, j As Integer = 0
        While i < bricks.Length
            While j < bricks(i).Count
                If bricks(i)(j) Then haswon = False
                j += 1
            End While
            j = 0
            i += 1
        End While
        If haswon Then
            gameprogress = 1
            MsgBox("You win!", MsgBoxStyle.Information, "Arkanoid")
            NewGame()
            gameprogress = 2
        End If
    End Sub
#End Region
    Private Sub Game_Engine()
        While True
            If mustclose Then Exit While
            Threading.Thread.Sleep(219)
            'Handle keypresses
            Select Case gameprogress
                Case 0
                    Exit Select
                Case 2
                    If (GetAsyncKeyState(Keys.Left) <> 0) And (paddle_p.X > 25) Then
                        paddle_p.X -= 20
                    End If
                    If (GetAsyncKeyState(Keys.Right) <> 0) And ((paddle_p.X + pWidth) < (Me.ClientSize.Width - 25)) Then
                        paddle_p.X += 20
                    End If
                Case 3
                    If GetAsyncKeyState(Keys.Y) <> 0 Then
                        mustclose = True
                        Dim dClose As New DNoParams(AddressOf Me.Close)
                        Me.Invoke(dClose)
                    ElseIf GetAsyncKeyState(Keys.N) <> 0 Then
                        gameprogress = 2
                    End If
            End Select
            If GetAsyncKeyState(Keys.X) <> 0 Then
                gameprogress = 3
            End If
            If GetAsyncKeyState(Keys.Control) <> 0 Then
                If GetAsyncKeyState(Keys.N) <> 0 Then
                    NewGame()
                End If
            End If
        End While
    End Sub
    Private Sub Game_Cycle()
        While True
            If mustclose Then Exit While
            Threading.Thread.Sleep(125)
            If gameprogress = 2 Then
                MoveBall()
            End If
            Me.Invalidate()
        End While
    End Sub
End Class
