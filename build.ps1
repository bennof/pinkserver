# Powershell Build System
# Copyright 2018 Benjmain 'Benno' Falkner  


param (
    [string]$new = "",
    [string]$cfginput = "project.cfg",
    [switch]$help = $false,
    [switch]$deps = $false,
    [switch]$build = $false,
    [switch]$run = $false
)

function mk_deps(){
    Write-Host "<<DEPS>>"
    $conf["deps"].Keys | foreach-object -process { 
        Write-Host "$_ :"$conf["deps"][$_]
        
    }
    return 0
}
function mk_build(){
    Write-Host "<<BUILD>>"
    $conf["build"].Keys | foreach-object -process {
        if ($_.Trim().EndsWith(".dll")) { # library
            Write-Host "csc -target:library -out:$_ $($conf['build'][$_])"
            Invoke-Expression "csc -target:library -out:$_ $($conf['build'][$_])" | Write-Host
        } elseif ($_.Trim().EndsWith(".exe")) { # executable
            Write-Host "csc -out:$_ $($conf['build'][$_])"
            Invoke-Expression "csc -out:$_ $($conf['build'][$_])" | Write-Host
        } else {
            Write-Error "ERROR: Unkown target: $_"
            return 1
        }
    }
    return 0
}
function mk_run(){
    return 0
}

if ($help) {
    return 0;
}

if (-not ([string]::IsNullOrEmpty($new) )) {
    write-host "Setup new project: $new."
    return 0;
}



# Read config
$conf=@{}
Try { 
    $group = ""
    Get-Content $cfginput | foreach-object -process { 
            $_ = $_.Trim()
            if ( $_.StartsWith("#") -ne $True ) {
                if ( $_.StartsWith("[") ) {
                    $group = $_.Trim("[","]")
                    $conf.Add($group, @{})
                } else {
                    $k = [regex]::split($_,'=');
                    if( ($k[0].CompareTo("") -ne 0)) {
                        if (-not ([string]::IsNullOrEmpty($group))) {
                            $conf.Get_Item($group).Add($k[0], $k[1])
                        } else {
                            $conf.Add($k[0].Trim(), $k[1].Trim())
                        } 
                    }
                }
            }
        }
} Catch { Write-Error "ERROR: Project file could not be read!"; return 1; }

#$conf | Format-Table

if ($deps) {
    if ( mk_deps -ne 0 ) { return 1 }
    return 0
}

if ($build) {
    if ( mk_deps -ne 0 ) { return 1 }
    if ( mk_build -ne 0 ) { return 1 }
    return 0;
}

if ($run) {
    if ( mk_deps -ne 0 ) { return 1 }
    if ( mk_build -ne 0 ) { return 1 }
    if ( mk_run -ne 0 ) { return 1 }
    return 0;
}

