#!groovy

def initializeEnvironment() {
  env.DRIVER_DISPLAY_NAME = 'C# Graph Extension'
  env.DRIVER_METRIC_TYPE = 'graph'

  env.GIT_SHA = "${env.GIT_COMMIT.take(7)}"
  env.GITHUB_PROJECT_URL = "https://${GIT_URL.replaceFirst(/(git@|http:\/\/|https:\/\/)/, '').replace(':', '/').replace('.git', '')}"
  env.GITHUB_BRANCH_URL = "${GITHUB_PROJECT_URL}/tree/${env.BRANCH_NAME}"
  env.GITHUB_COMMIT_URL = "${GITHUB_PROJECT_URL}/commit/${env.GIT_COMMIT}"

  if (env.OS_VERSION.split('/')[0] == 'win') {    
    env.HOME = 'C:\\Users\\Admin'
    env.HOME_WSL = '/mnt/c/Users/Admin'

    powershell label: 'Copy SSL files', script: '''
      wsl bash --login -c "cp -r $env:HOME_WSL/ccm/ssl `$HOME/ssl"
    '''
    
    powershell label: 'Download Apache Cassandra&reg; or DataStax Enterprise', script: '''
      rm $Env:HOME\\environment.txt
      rm $Env:HOME\\driver-environment.ps1

      wsl bash --login -c "$Env:CCM_ENVIRONMENT_SHELL_WINDOWS $Env:SERVER_VERSION"
      wsl bash --login -c "cp ~/environment.txt $ENV:HOME_WSL"
      
      $data = get-content "$Env:HOME\\environment.txt"
      $data = $data -replace "`n","`r`n"
      $newData = ""
	    $data | foreach {
          $v1,$v2 = $_.split("=",2)
          echo "1: $v1 2: $v2"
          $newData += "`r`n`$Env:$v1='$v2'"
      }
      $newData += "`r`n`$Env:CASSANDRA_VERSION=`$Env:CCM_CASSANDRA_VERSION"
      "$newData" | Out-File -filepath $Env:HOME\\driver-environment.ps1
    '''
    
    if (env.SERVER_VERSION.split('-')[0] == 'dse') {
      powershell label: 'Update environment for DataStax Enterprise', script: '''
          . $Env:HOME\\driver-environment.ps1

          $newData = "`r`n`$Env:DSE_BRANCH=`"$Env:CCM_BRANCH`""
          $newData += "`r`n`$Env:DSE_VERSION=`"$Env:CCM_VERSION`""
          $newData += "`r`n`$Env:DSE_INITIAL_IPPREFIX=`"127.0.0.`""
          $newData += "`r`n`$Env:DSE_IN_REMOTE_SERVER=`"false`""

          "$newData" | Out-File -filepath $Env:HOME\\driver-environment.ps1 -append
      '''

      if (env.SERVER_VERSION.split('-')[1] == '6.0') {
        powershell label: 'Update environment for DataStax Enterprise v6.0.x', script: '''
        . $Env:HOME\\driver-environment.ps1

        echo "Setting DSE 6.0 install-dir"
        "`r`n`$Env:DSE_PATH=`"$Env:CCM_INSTALL_DIR`"" | Out-File -filepath $Env:HOME\\driver-environment.ps1 -append
        '''
      }
    }

    powershell label: 'Set additional environment variables for windows tests', script: '''
      $newData = "`r`n`$Env:PATH+=`";$env:JAVA_HOME\\bin`""
      $newData += "`r`n`$Env:SIMULACRON_PATH=`"$Env:SIMULACRON_PATH_WINDOWS`""
      $newData += "`r`n`$Env:CCM_USE_WSL=`"true`""
      $newData += "`r`n`$Env:CCM_SSL_PATH=`"/root/ssl`""

      "$newData" | Out-File -filepath $Env:HOME\\driver-environment.ps1 -append
    '''

    powershell label: 'Display .NET and environment information', script: '''
      # Load CCM and driver configuration environment variables
      cat $Env:HOME\\driver-environment.ps1
      . $Env:HOME\\driver-environment.ps1

      dotnet --version

      gci env:* | sort-object name
    '''
  } else {
    sh label: 'Copy SSL files', script: '''#!/bin/bash -le
      cp -r ${HOME}/ccm/ssl $HOME/ssl
    '''

    sh label: 'Download Apache Cassandra&reg; or DataStax Enterprise', script: '''#!/bin/bash -le
      . ${CCM_ENVIRONMENT_SHELL} ${SERVER_VERSION}

      echo "CASSANDRA_VERSION=${CCM_CASSANDRA_VERSION}" >> ${HOME}/environment.txt
    '''

    if (env.SERVER_VERSION.split('-')[0] == 'dse') {
      sh label: 'Update environment for DataStax Enterprise', script: '''#!/bin/bash -le
        # Load CCM environment variables
        set -o allexport
        . ${HOME}/environment.txt
        set +o allexport

        cat >> ${HOME}/environment.txt << ENVIRONMENT_EOF
CCM_PATH=${HOME}/ccm
DSE_BRANCH=${CCM_BRANCH}
DSE_INITIAL_IPPREFIX=127.0.0.
DSE_IN_REMOTE_SERVER=false
ENVIRONMENT_EOF
      '''

      if (env.SERVER_VERSION.split('-')[1] == '6.0') {
        sh label: 'Update environment for DataStax Enterprise v6.0.x', script: '''#!/bin/bash -le
          # Load CCM and driver configuration environment variables
          set -o allexport
          . ${HOME}/environment.txt
          set +o allexport

          echo "DSE_PATH=${CCM_INSTALL_DIR}" >> ${HOME}/environment.txt
        '''
      }
    }

    sh label: 'Display .NET and environment information', script: '''#!/bin/bash -le
      # Load CCM and driver configuration environment variables
      set -o allexport
      . ${HOME}/environment.txt
      set +o allexport

      if [ ${DOTNET_VERSION} = 'mono' ]; then
        mono --version
      else
        dotnet --version
      fi
      printenv | sort
    '''
  }
}

def installDependencies() {
  if (env.OS_VERSION.split('/')[0] == 'win') {
    powershell label: 'Download saxon', script: '''
      [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
      mkdir saxon
      Invoke-WebRequest -OutFile saxon/saxon9he.jar -Uri https://repo1.maven.org/maven2/net/sf/saxon/Saxon-HE/9.8.0-12/Saxon-HE-9.8.0-12.jar
    '''
  } else {
    sh label: 'Download saxon', script: '''#!/bin/bash -le
      mkdir saxon
      curl -L -o saxon/saxon9he.jar https://repo1.maven.org/maven2/net/sf/saxon/Saxon-HE/9.8.0-12/Saxon-HE-9.8.0-12.jar
    '''

    if (env.DOTNET_VERSION == 'mono') {
      sh label: 'Install required packages for mono builds', script: '''#!/bin/bash -le
        # Define alias for Nuget
        nuget() {
          mono /usr/local/bin/nuget.exe "$@"
        }
        export -f nuget

        nuget install NUnit.Runners -Version 3.11.1 -OutputDirectory testrunner
      '''
    }
  }
}

def buildDriver() {
  if (env.OS_VERSION.split('/')[0] == 'win') {
    powershell label: "Install required packages and build the driver for ${env.DOTNET_VERSION}", script: '''
        dotnet restore src
        dotnet restore src
      '''
  } else {
    if (env.DOTNET_VERSION == 'mono') {
      sh label: 'Build the driver for mono', script: '''#!/bin/bash -le
        msbuild /t:restore /v:m src/Cassandra.DataStax.Graph.sln
        msbuild /p:Configuration=Release /v:m /p:DynamicConstants=LINUX src/Cassandra.DataStax.Graph.sln
      '''
    } else {
      sh label: "Work around nuget issue", script: '''#!/bin/bash -le
        mkdir -p /tmp/NuGetScratch
        chmod -R ugo+rwx /tmp/NuGetScratch
      '''
      sh label: "Install required packages and build the driver for ${env.DOTNET_VERSION}", script: '''#!/bin/bash -le
        dotnet restore src || true
        dotnet restore src
      '''
    }
  }
}

def executeUnitTests(perCommitSchedule) {
    
  if (env.OS_VERSION.split('/')[0] == 'win') {
    catchError(stageResult: 'FAILURE') {
      powershell label: "Execute unit tests for ${env.DOTNET_VERSION}", script: '''
        . $env:HOME\\Documents\\WindowsPowerShell\\Microsoft.PowerShell_profile.ps1
        . $Env:HOME\\driver-environment.ps1
        dotnet test src/Cassandra.DataStax.Graph.Test.Unit/Cassandra.DataStax.Graph.Test.Unit.csproj -f $DOTNET_VERSION  -c Release --logger "xunit;LogFilePath=../../TestResultUnit_xunit.xml" -- RunConfiguration.TargetPlatform=x64
      '''
    }
    powershell label: 'Convert the test results using saxon', script: '''
      java -jar saxon/saxon9he.jar -o:TestResultUnit.xml TestResultUnit_xunit.xml tools/JUnitXml.xslt
    '''
  } else {
    if (env.DOTNET_VERSION == 'mono') {
      catchError(stageResult: 'FAILURE') {
        sh label: 'Execute unit tests for mono', script: '''#!/bin/bash -le
          # Load CCM and driver configuration environment variables
          set -o allexport
          . ${HOME}/environment.txt
          set +o allexport

          mono ./testrunner/NUnit.ConsoleRunner.3.11.1/tools/nunit3-console.exe src/Cassandra.DataStax.Graph.Test.Unit/bin/Release/net461/Cassandra.DataStax.Graph.Test.Unit.dll --where "cat != long && cat != memory" --labels=All --result:"TestResultUnit_nunit.xml"
        '''
      }
      sh label: 'Convert the test results using saxon', script: '''#!/bin/bash -le
        java -jar saxon/saxon9he.jar -o:TestResultUnit.xml TestResultUnit_nunit.xml tools/nunit3-junit.xslt
      '''
    } else {
      catchError(stageResult: 'FAILURE') {
        sh label: "Execute unit tests for ${env.DOTNET_VERSION}", script: '''#!/bin/bash -le
          # Load CCM and driver configuration environment variables
          set -o allexport
          . ${HOME}/environment.txt
          set +o allexport

          dotnet test src/Cassandra.DataStax.Graph.Test.Unit/Cassandra.DataStax.Graph.Test.Unit.csproj -f $DOTNET_VERSION  -c Release --logger "xunit;LogFilePath=../../TestResultUnit_xunit.xml"
        '''
      }
      sh label: 'Convert the test results using saxon', script: '''#!/bin/bash -le
        java -jar saxon/saxon9he.jar -o:TestResultUnit.xml TestResultUnit_xunit.xml tools/JUnitXml.xslt
      '''
    } 
  }
}

def executeIntegrationTests(perCommitSchedule) {
    
  if (env.OS_VERSION.split('/')[0] == 'win') {    
    catchError(stageResult: 'FAILURE') {
      powershell label: "Execute integration tests for ${env.DOTNET_VERSION}", script: '''
        . $env:HOME\\Documents\\WindowsPowerShell\\Microsoft.PowerShell_profile.ps1
        . $Env:HOME\\driver-environment.ps1
        dotnet test src/Cassandra.DataStax.Graph.Test.Integration/Cassandra.DataStax.Graph.Test.Integration.csproj -f $DOTNET_VERSION  -c Release --logger "xunit;LogFilePath=../../TestResultIntegration_xunit.xml" -- RunConfiguration.TargetPlatform=x64
      '''
    }
    powershell label: 'Convert the test results using saxon', script: '''
      java -jar saxon/saxon9he.jar -o:TestResultIntegration.xml TestResultIntegration_xunit.xml tools/JUnitXml.xslt
    '''
  } else {
    if (env.DOTNET_VERSION == 'mono') {
      catchError(stageResult: 'FAILURE') {
        sh label: 'Execute integration tests for mono', script: '''#!/bin/bash -le
          # Load CCM and driver configuration environment variables
          set -o allexport
          . ${HOME}/environment.txt
          set +o allexport

          mono ./testrunner/NUnit.ConsoleRunner.3.11.1/tools/nunit3-console.exe src/Cassandra.DataStax.Graph.Test.Integration/bin/Release/net461/Cassandra.DataStax.Graph.Test.Integration.dll --where "cat != long && cat != memory" --labels=All --result:"TestResultIntegration_nunit.xml"
        '''
      }
      sh label: 'Convert the test results using saxon', script: '''#!/bin/bash -le
        java -jar saxon/saxon9he.jar -o:TestResultIntegration.xml TestResultIntegration_nunit.xml tools/nunit3-junit.xslt
      '''
    } else {
      catchError(stageResult: 'FAILURE') {
        sh label: "Execute integration tests for ${env.DOTNET_VERSION}", script: '''#!/bin/bash -le
          # Load CCM and driver configuration environment variables
          set -o allexport
          . ${HOME}/environment.txt
          set +o allexport

          dotnet test src/Cassandra.DataStax.Graph.Test.Integration/Cassandra.DataStax.Graph.Test.Integration.csproj -f $DOTNET_VERSION  -c Release --logger "xunit;LogFilePath=../../TestResultIntegration_xunit.xml"
        '''
      }
      sh label: 'Convert the test results using saxon', script: '''#!/bin/bash -le
        java -jar saxon/saxon9he.jar -o:TestResultIntegration.xml TestResultIntegration_xunit.xml tools/JUnitXml.xslt
      '''
    } 
  }
}

def notifySlack(status = 'started') {
  // Set the global pipeline scoped environment (this is above each matrix)
  env.BUILD_STATED_SLACK_NOTIFIED = 'true'

  def osVersionDescription = 'Ubuntu'
  if (params.CI_SCHEDULE_OS_VERSION == 'win/cs') {
    osVersionDescription = 'Windows'
  }

  def buildType = 'Per-Commit'
  if (params.CI_SCHEDULE != 'DEFAULT-PER-COMMIT') {
    buildType = "${params.CI_SCHEDULE.toLowerCase().capitalize()}-${osVersionDescription}"
  }

  def color = 'good' // Green
  if (status.equalsIgnoreCase('aborted')) {
    color = '#808080' // Grey
  } else if (status.equalsIgnoreCase('unstable')) {
    color = 'warning' // Orange
  } else if (status.equalsIgnoreCase('failed')) {
    color = 'danger' // Red
  } else if (status.equalsIgnoreCase("started")) {
    color = '#fde93f' // Yellow
  }

  def message = """<${env.RUN_DISPLAY_URL}|Build #${env.BUILD_NUMBER}> ${status} for ${env.DRIVER_DISPLAY_NAME}
[${buildType}] <${env.GITHUB_BRANCH_URL}|${env.BRANCH_NAME}> <${env.GITHUB_COMMIT_URL}|${env.GIT_SHA}>"""

  if (!status.equalsIgnoreCase('Started')) {
    message += """
${status} after ${currentBuild.durationString - ' and counting'}"""
  }

  slackSend color: "${color}",
            channel: "#csharp-driver-dev-bots",
            message: "${message}"
}

def submitCIMetrics(buildType) {
  long durationMs = currentBuild.duration
  long durationSec = durationMs / 1000
  long nowSec = (currentBuild.startTimeInMillis + durationMs) / 1000
  def branchNameNoPeriods = env.BRANCH_NAME.replaceAll('\\.', '_')
  def durationMetric = "okr.ci.csharp.${env.DRIVER_METRIC_TYPE}.${buildType}.${branchNameNoPeriods} ${durationSec} ${nowSec}"

  timeout(time: 1, unit: 'MINUTES') {
    withCredentials([string(credentialsId: 'lab-grafana-address', variable: 'LAB_GRAFANA_ADDRESS'),
                     string(credentialsId: 'lab-grafana-port', variable: 'LAB_GRAFANA_PORT')]) {
      withEnv(["DURATION_METRIC=${durationMetric}"]) {
        sh label: 'Send runtime metrics to labgrafana', script: '''#!/bin/bash -le
          echo "${DURATION_METRIC}" | nc -q 5 ${LAB_GRAFANA_ADDRESS} ${LAB_GRAFANA_PORT}
        '''
      }
    }
  }
}

@NonCPS
def getChangeLog() {
    def log = ""
    def changeLogSets = currentBuild.changeSets
    for (int i = 0; i < changeLogSets.size(); i++) {
        def entries = changeLogSets[i].items
        for (int j = 0; j < entries.length; j++) {
            def entry = entries[j]
            log += "  * ${entry.msg} by ${entry.author} <br>"
        }
    }
    return log;
  }

def describePerCommitStage() {
  script {
    currentBuild.displayName = "#${env.BUILD_NUMBER} - Per-Commit (${env.GIT_SHA})"
    currentBuild.description = "Changelog:<br>${getChangeLog()}".take(250)
  }
}

def describeScheduledTestingStage() {
  script {
    def type = params.CI_SCHEDULE.toLowerCase().capitalize()
    def serverVersionDescription = "almost all server version(s) in the matrix"
    def osVersionDescription = 'Ubuntu 18.04 LTS'
    if (env.OS_VERSION == 'win/cs') {
      osVersionDescription = 'Windows 10'
    }    
    currentBuild.displayName = "#${env.BUILD_NUMBER} - ${type} (${osVersionDescription})"
    currentBuild.description = "${type} scheduled testing for ${serverVersionDescription} on ${osVersionDescription}"
  }
}

pipeline {
  agent none

  // Global pipeline timeout
  options {
    disableConcurrentBuilds()
    timeout(time: 10, unit: 'HOURS')
    buildDiscarder(logRotator(artifactNumToKeepStr: '10', // Keep only the last 10 artifacts
                              numToKeepStr: '50'))        // Keep only the last 50 build records
  }

  parameters {
    choice(
      name: 'CI_SCHEDULE',
      choices: ['DEFAULT-PER-COMMIT'],
      description: '''<table style="width:100%">
                        <col width="20%">
                        <col width="80%">
                        <tr>
                          <th align="left">Choice</th>
                          <th align="left">Description</th>
                        </tr>
                        <tr>
                          <td><strong>ubuntu/bionic64/csharp-driver</strong></td>
                          <td>Ubuntu 18.04 LTS x86_64</td>
                        </tr>
                        <tr>
                          <td><strong>win/cs</strong></td>
                          <td>Windows 10</td>
                        </tr>
                      </table>''')
    choice(
      name: 'CI_SCHEDULE_OS_VERSION',
      choices: ['DEFAULT-PER-COMMIT', 'ubuntu/bionic64/csharp-driver', 'win/cs'],
      description: 'CI testing operating system version to utilize')
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    SIMULACRON_PATH = '/home/jenkins/simulacron.jar'
    SIMULACRON_PATH_WINDOWS = 'C:\\Users\\Admin\\simulacron.jar'
    CCM_ENVIRONMENT_SHELL = '/usr/local/bin/ccm_environment.sh'
    CCM_ENVIRONMENT_SHELL_WINDOWS = '/mnt/c/Users/Admin/ccm_environment.sh'
  }

  stages {
    stage('Per-Commit') {
      when {
        beforeAgent true
        allOf {
          expression { params.CI_SCHEDULE == 'DEFAULT-PER-COMMIT' }
          not { buildingTag() }
        }
      }

      environment {
        OS_VERSION = 'ubuntu/bionic64/csharp-driver'
      }

      matrix {
        axes {
          axis {
            name 'SERVER_VERSION'
            values 'dse-5.1', // latest 5.1.x DataStax Enterprise
                  'dse-6.0',
                  'dse-6.7', // latest 6.7.x DataStax Enterprise
                  'dse-6.8.0' // 6.8.0 current DataStax Enterprise
          }
          axis {
            name 'DOTNET_VERSION'
            values 'mono', 'netcoreapp2.1'
          }
        }
        excludes {
          exclude {
            axis {
              name 'DOTNET_VERSION'
              values 'mono'
            }
            axis {
              name 'SERVER_VERSION'
              values 'dse-6.0'
            }
          }
        }

        agent {
          label "${OS_VERSION}"
        }

        stages {
          stage('Initialize-Environment') {
            steps {
              initializeEnvironment()
              script {
                if (env.BUILD_STATED_SLACK_NOTIFIED != 'true') {
                  notifySlack()
                }
              }
            }
          }
          stage('Describe-Build') {
            steps {
              describePerCommitStage()
            }
          }
          stage('Install-Dependencies') {
            steps {
              installDependencies()
            }
          }
          stage('Build-Driver') {
            steps {
              buildDriver()
            }
          }
          stage('Execute-Unit-Tests') {
            steps {
              executeUnitTests(true)
            }
            post {
              always {
                junit testResults: '**/TestResultUnit.xml'
              }
            }
          }
          stage('Execute-Integration-Tests') {
            steps {
              executeIntegrationTests(true)
            }
            post {
              always {
                junit testResults: '**/TestResultIntegration.xml'
              }
            }
          }
        }
      }
      post {
        always {
          node('master') {
            submitCIMetrics('commit')
          }
        }
        aborted {
          notifySlack('aborted')
        }
        success {
          notifySlack('completed')
        }
        unstable {
          notifySlack('unstable')
        }
        failure {
          notifySlack('FAILED')
        }
      }
    }
  }
}