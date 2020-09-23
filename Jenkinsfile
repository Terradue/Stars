pipeline {
  agent any
  environment {
      VERSION = getVersionFromCsProj('src/Stars.Service/Terradue.Stars.Service.csproj')
      VERSION_TYPE = getTypeOfVersion(env.BRANCH_NAME)
      CONFIGURATION = getConfiguration(env.BRANCH_NAME)
  }
  stages {
    stage('Build') {
      agent { 
          docker { 
              image 'mcr.microsoft.com/dotnet/core/sdk:3.1-bionic' 
          } 
      }
      options {
        skipDefaultCheckout()
      }
      steps {
        ws("/source") {
          checkout scm
          echo "Build .NET application"
          sh "dotnet restore src/"
          sh "dotnet build -c ${env.CONFIGURATION} src/"
          stash includes: 'src/Terradue.Stars.*/bin/**', name: 'terradue-stars-build'
        }
      }
    }
    stage('Package as RPM') {
      agent { 
        docker { 
            image 'alectolytic/rpmbuilder:centos-7' 
        } 
      }
      steps {
        unstash name: 'terradue-stars-build'
        script{
          def sdf = sh(returnStdout: true, script: 'date -u +%Y%m%dT%H%M%S').trim()
          if (env.BRANCH_NAME == 'master') 
            env.release = env.BUILD_NUMBER
          else
            env.release = "SNAPSHOT" + sdf
          def descriptor = readDescriptor()
        }
        sh 'mkdir -p $WORKSPACE/build/{BUILD,RPMS,SOURCES,SPECS,SRPMS}'
        sh 'mkdir -p $WORKSPACE/build/SOURCES/usr/lib/stars-console'
        sh 'cp -r src/Terradue.Stars.Console/bin/Debug/*/* $WORKSPACE/build/SOURCES/usr/lib/stars/'
        sh 'mkdir -p $WORKSPACE/build/SOURCES/usr/bin'
        sh 'cp src/main/scripts/stars-console $WORKSPACE/build/SOURCES/usr/bin'
        sh 'cp src/main/scripts/stars-console $WORKSPACE/build/SOURCES/'
        sh 'cp stars-console.spec $WORKSPACE/build/SPECS/stars-console.spec'
        sh 'spectool -g -R --directory $WORKSPACE/build/SOURCES $WORKSPACE/build/SPECS/stars-console.spec'
        echo "Build package"
        sh "rpmbuild --define \"_topdir $WORKSPACE/build\" -ba --define '_branch ${env.BRANCH_NAME}' --define '_version ${env.VERSION}' --define '_release ${env.release}' $WORKSPACE/build/SPECS/stars-console.spec"
        sh "rpm -qpl $WORKSPACE/build/RPMS/*/*.rpm"
        sh 'rm -f $WORKSPACE/build/SOURCES/stars-console'
        sh "tar -cvzf stars-console-${env.VERSION}-${env.release}.tar.gz -C $WORKSPACE/build/SOURCES/ ."
        archiveArtifacts artifacts: 'build/RPMS/**/*.rpm,stars-console-*.tar.gz', fingerprint: true
        stash includes: 'stars-console-*.tar.gz', name: 'stars-console-tgz'
        stash includes: 'build/RPMS/**/*.rpm', name: 'stars-console-rpm'
      }
    }
    stage('Publish Artifacts') {
      agent { node { label 'artifactory' } }
      when{
        branch 'master'
      }
      steps {
        echo 'Deploying'
        unstash name: 'stars-console-rpm'
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
            unstash name: 'stars-console-tgz'
            script {
              def opensearchclienttgz = findFiles(glob: "stars-console-*.tar.gz")
              def descriptor = readDescriptor()
              def testsuite = docker.build(descriptor.docker_image_name, "--build-arg OPENSEARCH_CLIENT_TGZ=${opensearchclienttgz[0].name} .")
              def mType=getTypeOfVersion(env.BRANCH_NAME)
              docker.withRegistry('https://registry.hub.docker.com', 'dockerhub-emmanuelmathot') {
                testsuite.push("${mType}${descriptor.version}")
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
  return xml.Project.PropertyGroup.Version.text()
}



