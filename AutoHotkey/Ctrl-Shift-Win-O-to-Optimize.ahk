#NoEnv
#SingleInstance Force

; FileEncoding, UTF-8  ; 設定檔案編碼為 UTF-8
SendMode Input
SetWorkingDir %A_ScriptDir%

; 應用程式 System Tray 圖示
Menu, Tray, Icon, Ctrl-Win-V-Paste-to-Translate.ico

; Ctrl+Win+O
^+#o::
    ; 獲取要優化的文本
    textToOptimize := Trim(Clipboard)
    if (textToOptimize != "")
    {
        ; 處理臨時文件
        tempFile := A_Temp "\openai-wrapper-optimize-request.txt"
        FileDelete, %tempFile%
        FileAppend, %textToOptimize%, %tempFile%, UTF-8-RAW

        ; 執行命令且完全隱藏視窗
        optimizedResponseFilePath := A_Temp "\openai-wrapper-optimize-response.txt"
        FileDelete, %optimizedResponseFilePath%
        RunWait, %ComSpec% /c %A_ScriptDir%\openai-wrapper.exe "%tempFile%" "%optimizedResponseFilePath%" optimize,, Hide

        ; 檢查命令執行是否成功
        if (ErrorLevel != 0) {
            MsgBox, Error: Failed to execute optimization command.
            return
        }

        ; 讀取回應文件
        FileRead, response, % "*P65001 " optimizedResponseFilePath
        response := Trim(response)  ; 移除前後空白

        ; 如果 response 不是空字串，就回覆優化結果
        if (response != "")
        {
            ; 使用剪貼簿方式回覆結果
            old_clip := ClipboardAll
            Clipboard := response
            SendInput, ^v
            Sleep, 100
            Clipboard := old_clip
        }
        else
        {
            MsgBox, Error: Failed to read response.
        }
    }
    else
    {
        MsgBox, Error: Clipboard is empty.
    }
return
