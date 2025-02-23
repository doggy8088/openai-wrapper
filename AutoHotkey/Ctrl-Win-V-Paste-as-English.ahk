#NoEnv
#SingleInstance Force

; FileEncoding, UTF-8  ; 設定檔案編碼為 UTF-8
SendMode Input
SetWorkingDir %A_ScriptDir%

; 應用程式 System Tray 圖示
Menu, Tray, Icon, Ctrl-Win-V-Paste-as-English.ico

; Ctrl+Win+V

<^<#v::
    ; 獲取要翻譯的文本
    textToTranslate := Trim(Clipboard)
    if (textToTranslate != "")
    {
        ; 臨時文件處理
        tempFile := A_Temp "\openai-wrapper-request.txt"
        FileDelete, %tempFile%
        FileAppend, %textToTranslate%, %tempFile%, UTF-8-RAW

        ; 執行命令且完全隱藏視窗
        responseFilePath := A_Temp "\openai-wrapper-response.txt"

        FileDelete, % responseFilePath
        RunWait, %ComSpec% /c %A_ScriptDir%\openai-wrapper.exe "%tempFile%" "openai-wrapper-response.txt",, Hide
        FileRead, response, % "*P65001 " responseFilePath

        response := Trim(response)  ; 移除前後空白
        ; MsgBox % response

        ; 如果 response 不是空字串，就回覆翻譯結果
        if (response != "")
        {
            ; MsgBox % response

            ; 最快速的方式，建議優先使用
            ; SendInput 會忽略 SetKeyDelay, 因為在這種發送模式中操作系統不支援延遲.
            SendInput % "{Raw}" Trim(response)

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
