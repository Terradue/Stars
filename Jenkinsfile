pipeline {
  agent any
  environment {
      VERSION_LIB = getVersionFromCsProj('src/Stars.Services/Terradue.Stars.Services.csproj')
      VERSION_TOOL = getVersionFromCsProj('src/Stars.Console/Terradue.Stars.Console.csproj')
      VERSION_TYPE = getTypeOfVersion(env.BRANCH_NAME)
      CONFIGURATION = getConfiguration(env.BRANCH_NAME)
      GITHUB_ORGANIZATION = 'Terradue'
      GITHUB_REPO = 'Stars'
  }
  stages {
    stage('.Net Core') {
      agent { 
          docker { 
              image 'mcr.microsoft.com/dotnet/sdk:5.0-buster-slim'
          } 
      }
      environment {
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
      }
      stages {
        stage("Build") {
          steps {
            echo "Build .NET application"
            sh "dotnet restore src/"
            sh "dotnet build -c ${env.CONFIGURATION} --no-restore  src/"
          }
        }
        stage("Make packages"){
          steps {
            script {
              def sdf = sh(returnStdout: true, script: 'date -u +%Y%m%dT%H%M%S').trim()
              if (env.BRANCH_NAME == 'master') 
                env.RELEASE = ""
              else
                env.RELEASE = "SNAPSHOT" + sdf
            }
            sh "dotnet tool restore"
            // sh "dotnet rpm -c ${env.CONFIGURATION} -r rhel.6-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            sh "dotnet rpm -c ${env.CONFIGURATION} -r centos.7-x64 -f netcoreapp3.1 --version-suffix '${env.RELEASE}' src/Stars.Console/Terradue.Stars.Console.csproj"
            // sh "dotnet rpm -c ${env.CONFIGURATION} -r rhel-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            // sh "dotnet deb -c ${env.CONFIGURATION} -r ubuntu.18.04-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            // sh "dotnet deb -c ${env.CONFIGURATION} -r ubuntu.19.04-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            // sh "dotnet deb -c ${env.CONFIGURATION} -r debian.9-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            // sh "dotnet zip -c ${env.CONFIGURATION} -r linux-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            sh "dotnet publish -f net5.0 -r linux-x64 -p:PublishSingleFile=true --self-contained true src/Stars.Console/Terradue.Stars.Console.csproj"
            stash name: 'stars-packages', includes: 'src/Stars.Console/bin/**/*.rpm'
            stash name: 'stars-rpms', includes: 'src/Stars.Console/bin/**/*.rpm'
            stash name: 'stars-exe', includes: 'src/Stars.Console/bin/**/publish/Stars'
            archiveArtifacts artifacts: 'src/Stars.Console/bin/**/publish/Stars,src/Stars.Console/bin/**/*.rpm,src/Stars.Console/bin/**/*.deb, src/Stars.Console/bin/**/*.zip', fingerprint: true
          }
        }
        stage('Publish NuGet') {
          when{
            branch 'master'
          }
          steps {
            withCredentials([string(credentialsId: 'nuget_token', variable: 'NUGET_TOKEN')]) {
              sh "dotnet publish src/Stars.Services -c ${env.CONFIGURATION} -f netstandard2.1"
              sh "dotnet pack src/Stars.Services -c ${env.CONFIGURATION} --include-symbols -o publish"
              sh "dotnet nuget push publish/*.nupkg --skip-duplicate -k $NUGET_TOKEN -s https://api.nuget.org/v3/index.json"
            }
          }
        }
      }
    }
    stage('Publish Artifacts') {
      agent { node { label 'artifactory' } }
      steps {
        echo 'Deploying'
        unstash name: 'stars-rpms'
        script {
            // Obtain an Artifactory server instance, defined in Jenkins --> Manage:
            def server = Artifactory.server "repository.terradue.com"

            // Read the upload specs:
            def uploadSpec = readFile 'artifactdeploy.json'

            // Upload files to Artifactory:
            def buildInfo = server.upload spec: uploadSpec

            // Publish the merged build-info to Artifactory
            server.publishBuildInfo buildInfo
        }
      }       
    }
    stage('Build & Publish Docker') {
      steps {
        script {
          unstash name: 'stars-packages'
          def starsrpm = findFiles(glob: "src/Stars.Console/bin/**/Stars.*.centos.7-x64.rpm")
          def descriptor = readDescriptor()
          sh "mv ${starsrpm[0].path} ."
          def mType=getTypeOfVersion(env.BRANCH_NAME)
          def testsuite = docker.build(descriptor.docker_image_name + ":${mType}${env.VERSION_TOOL}", "--build-arg STARS_RPM=${starsrpm[0].name} .")
          testsuite.tag("${mType}latest")
          docker.withRegistry('https://registry.hub.docker.com', 'dockerhub') {
            testsuite.push("${mType}${env.VERSION_TOOL}")
            testsuite.push("${mType}latest")
          }
        }
      }
    }  
    stage('Create Release') {
      agent { 
          docker { 
              image 'golang'
              args '-u root'
          } 
      }
      when {
        branch 'master'
      }
      steps {
        withCredentials([string(credentialsId: '11f06c51-2f47-43be-aef4-3e4449be5cf0', variable: 'GITHUB_TOKEN')]) {
          unstash name: 'stars-exe'
          sh "go get github.com/github-release/github-release"
          // echo "Deleting release from github before creating new one"
          // sh "github-release delete --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL}"

          echo "Creating a new release in github"
          sh "github-release release --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL} --name v${env.VERSION_TOOL}"

          echo "Uploading the artifacts into github"
          sh "github-release upload --user ${env.GITHUB_ORGANIZATION} --repo ${env.GITHUB_REPO} --tag ${env.VERSION_TOOL} --name Stars-linux-x64  --file src/Stars.Console/bin/Release/net5.0/linux-x64/publish/Stars"
        }
      }
        
      
    }  
  }
}

def getTypeOfVersion(branchName) {
  def matcher = (branchName =~ /master/)
  if (matcher.matches())
    return ""
  
  return "dev"
}

def getConfiguration(branchName) {
  def matcher = (branchName =~ /master/)
  if (matcher.matches())
    return "Release"
  
  return "Debug"
}

def readDescriptor (){
  return readYaml(file: 'build.yml')
}

def getVersionFromCsProj (csProjFilePath){
  def file = readFile(csProjFilePath) 
  def xml = new XmlSlurper().parseText(file)
  return xml.PropertyGroup.Version[0].text()
}



