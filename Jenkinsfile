pipeline {
  agent any
  environment {
      VERSION_LIB = getVersionFromCsProj('src/Stars.Services/Terradue.Stars.Services.csproj')
      VERSION_TOOL = getVersionFromCsProj('src/Stars.Console/Terradue.Stars.Console.csproj')
      VERSION_TYPE = getTypeOfVersion(env.BRANCH_NAME)
      CONFIGURATION = getConfiguration(env.BRANCH_NAME)
  }
  stages {
    stage('.Net Core') {
      agent { 
          docker { 
              image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
          } 
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
                env.RELEASE = env.BUILD_NUMBER
              else
                env.RELEASE = "SNAPSHOT" + sdf
            }
            sh "dotnet tool restore"
            sh "dotnet rpm -c ${env.CONFIGURATION} -r centos.7-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            sh "dotnet zip -c ${env.CONFIGURATION} -r linux-x64 -f netcoreapp3.1 --version-suffix ${env.RELEASE} src/Stars.Console/Terradue.Stars.Console.csproj"
            stash name: 'stars-packages', includes: 'src/Stars.Console/bin/**/*'
            stash name: 'stars-rpms', includes: 'src/Stars.Console/bin/**/*.rpm'
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
          def testsuite = docker.build(descriptor.docker_image_name, "--no-cache --build-arg STARS_RPM=${starsrpm[0].name} .")
          def mType=getTypeOfVersion(env.BRANCH_NAME)
          docker.withRegistry('https://registry.hub.docker.com', 'dockerhub') {
            testsuite.push("${mType}${env.VERSION_TOOL}")
            testsuite.push("${mType}latest")
          }
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
  return xml.PropertyGroup.Version.text()
}



