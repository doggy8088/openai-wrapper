#NoEnv
#SingleInstance Force

; FileEncoding, UTF-8  ; 設定檔案編碼為 UTF-8
SendMode Input
SetWorkingDir %A_ScriptDir%

; 應用程式 System Tray 圖示
Menu, Tray, Icon, Ctrl-Win-V-Paste-to-Translate.ico

; Ctrl+Win+V

^+#o::
    ; 獲取要翻譯的文本
    textToTranslate := Trim(Clipboard)
    if (textToTranslate != "")
    {
        ; 臨時文件處理
        tempFile := A_Temp "\openai-wrapper-optimize-request.txt"
        FileDelete, %tempFile%
        FileAppend, %textToTranslate%, %tempFile%, UTF-8-RAW

        ; 執行命令且完全隱藏視窗
        responseFilePath := A_Temp "\openai-wrapper-optimize-response.txt"

        FileDelete, % responseFilePath
        RunWait, %ComSpec% /c %A_ScriptDir%\openai-wrapper.exe "%tempFile%" "openai-wrapper-optimize-response.txt" optimize,, Hide
        FileRead, response, % "*P65001 " responseFilePath

        response := Trim(response)  ; 移除前後空白
        ; MsgBox % response

        ; 如果 response 不是空字串，就回覆翻譯結果
        if (response != "")
        {
            ; MsgBox % response

            ; 最快速的方式，建議優先使用
            ; SendInput 會忽略 SetKeyDelay, 因為在這種發送模式中操作系統不支援延遲.
            ; 方法一：使用 {Raw} 模式 (你目前的方法)
            ; SendInput % "{Raw}" response

            ; 方法二：使用 SendInput, 直接傳遞文字
            ; SendInput, %response%

            ; 方法三：使用 Send 函式，較慢但有時更可靠
            ; Send, %response%

            ; 方法四：結合 ClipBoard 的方式 (最可靠，但會改變剪貼簿)
            old_clip := ClipboardAll
            Clipboard := response
            SendInput, ^v
            Sleep, 100
            Clipboard := old_clip

            ; ; 方法五：使用 ControlSend 發送到活動視窗
            ; WinGetTitle, active_title, A
            ; ControlSend, , %response%, %active_title%

            ; 適合需要原始文字輸入的情況
            ; SetKeyDelay, 0  ; 設定按鍵延遲為 0
            ; SendRaw % Clipboard
        }
        else
        {
            MsgBox, Failed to read response
        }
    }
return
