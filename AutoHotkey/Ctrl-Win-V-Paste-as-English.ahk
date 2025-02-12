#NoEnv

FileEncoding, UTF-8  ; 設定檔案編碼為 UTF-8
SendMode Input
SetWorkingDir %A_ScriptDir%

Menu, Tray, Icon, Ctrl-Win-V-Paste-as-English.ico

<^<#v::
    ; 原始剪貼簿備份
    originalClipboard := ClipboardAll

    ; 獲取要翻譯的文本
    textToTranslate := Trim(Clipboard)
    if (textToTranslate != "")
    {
        ; 臨時文件處理
        tempFile := A_Temp "\openai-wrapper-request.txt"
        FileDelete, %tempFile%
        FileAppend, %textToTranslate%, %tempFile%, UTF-8-RAW

        ; 執行命令且完全隱藏視窗
        FileDelete, %A_Temp%\openai-wrapper-response.txt
        RunWait, %ComSpec% /c openai-wrapper.exe %tempFile% > "%A_Temp%\openai-wrapper-response.txt",, Hide
        FileRead, response, %A_Temp%\openai-wrapper-response.txt

        response := Trim(response)  ; 移除前後空白
        ; MsgBox % response

        ; 如果 response 不是空字串，就回覆翻譯結果
        if (response != "")
        {
            ; MsgBox % response

            ; 更新剪貼簿並貼上
            Clipboard := response
            ; ClipWait, 2  ; 等待剪貼簿更新，最多2秒

            if ErrorLevel  ; 如果剪貼簿更新失敗
            {
                MsgBox, Failed to update clipboard
                return
            }

            ; 最快速的方式，建議優先使用
            SetKeyDelay, 0  ; 設定按鍵延遲為 0
            SendInput % Clipboard

            ; 適合需要原始文字輸入的情況
            ; SendRaw % Clipboard
        }
        else
        {
            MsgBox, Failed to read response
        }
    }

    ; 恢復原始剪貼簿內容
    Clipboard := originalClipboard
    originalClipboard := ""  ; 釋放記憶體
return
