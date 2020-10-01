pipeline {
	agent none
	stages {
		stage('Initialisation') {
			agent { node { label 'windows-node-3' }}
			steps {
				dir('') {
					script {
						env.version_short = (bat(script: "@echo off && gitversion /showvariable MajorMinorPatch", returnStdout: true)).trim()
						env.version = (bat(script: '@echo off && gitversion /showvariable FullSemVer', returnStdout: true)).trim()
						env.AssemblySemFileVer = (bat(script: '@echo off && gitversion /showvariable AssemblySemFileVer', returnStdout: true)).trim()
						env.AssemblySemVer = (bat(script: '@echo off && gitversion /showvariable AssemblySemVer', returnStdout: true)).trim()

						env.git_branch_ugcs_telemetry_viewer = "${GIT_BRANCH}"
						env.git_commit_ugcs_telemetry_viewer = "${GIT_COMMIT}"
					}
				}
			}
		}
		stage('Building & Publishing') {
			agent { node { label 'windows-node-3' }}
			steps {
				bat ''' 
					mkdir build
					cd src
					dotnet publish TelemetryViewer.sln -r win-x64 --self-contained true -p:Configuration=Release;Version="%version%";FileVersion="%AssemblySemFileVer%";AssemblyVersion="%AssemblySemVer%"
					if ERRORLEVEL 1 exit 1
					"%ARCHIVATOR_7z_PATH%/7z.exe" a -tzip %WORKSPACE%/build/ugcs-telemetry-viewer-%version%.zip %WORKSPACE%/src/TelemetryViewer/bin/x64/Release/netcoreapp3.1/win-x64/publish/* 
					if ERRORLEVEL 1 exit 1 '''

				cifsPublisher publishers: [[
					configName: 'binaries_repo',
					verbose: true,
					transfers: [
						[
							cleanRemote: false, excludes: '', 
							remoteDirectory: "ugcs-telemetry-viewer/${version_short}/${version}",
							removePrefix: 'build',
							sourceFiles: 'build/*.zip'
						], [
							cleanRemote: false, excludes: '', 
							remoteDirectory: "ugcs-telemetry-viewer/${version_short}/latest",
							removePrefix: 'build',
							sourceFiles: 'build/*.zip'
						]]
				]]
			}
		}
	}
	post { 
		always {
			slackSend message: "Build ugcs-telemetry-viewer ${version} - ${currentBuild.result}. (<${env.BUILD_URL}|Open>)"
		}
		success { notifyBuild('SUCCESSFUL') }
		failure { notifyBuild('FAILED') }
		aborted { notifyBuild('FAILED') }
	}
	options {
		buildDiscarder(logRotator(numToKeepStr:'10'))
		timeout(time: 30, unit: 'MINUTES')
	}
}

def notifyBuild(String buildStatus) {
	buildStatus =  buildStatus ?: 'SUCCESSFUL'

	def subject = "Build ugcs-telemetry-viewer ${version} - ${buildStatus}."
	def summary = "${subject} (${env.BUILD_URL})"
	def details = """
<html>
	<body>
		<article>
			<h3>Build ugcs-telemetry-viewer ${version} - ${buildStatus}.</h3>
		</article>
		<article>
			<h3>Summary</h3>
			<p>
				<table>
					<col width="60">
					<col width="300">
					<tr>
						<td>Git branch name</td>
						<td>
							<a href="https://bitbucket.org/sphengineering/ugcs-telemetry-viewer/branch/${git_branch_ugcs_telemetry_viewer}">${git_branch_ugcs_telemetry_viewer}</a>
						</td>
					</tr>
					<tr>
						<td>Git revision</td>
						<td>
							<a href="https://bitbucket.org/sphengineering/ugcs-telemetry-viewer.git/commits/${git_commit_ugcs_telemetry_viewer}">${git_commit_ugcs_telemetry_viewer}</a>
						</td>
					</tr>
					<tr>
						<td>Build logs</td>
						<td><a href="${env.BUILD_URL}">check build logs</a></td>
					</tr>
					<tr>
						<td>Synplicity path</td>
						<td>"Binaries/ugcs-telemetry-viewer/${version_short}"</td>
					</tr>
				</table>
			</p>
		</article>
		<article>
			<h3>Changelogs</h3>
			<p>${changeString}</p>
		</article>
	</body> 
</html>"""

	bitbucketStatusNotify(buildState: buildStatus)
	emailext(
		subject: subject, mimeType: 'text/html', body: details,
		to: 'abykov@ugcs.com, morekhov@ugcs.com, nselivanov@ugcs.com, mbarabanova@ugcs.com, kkalnins@ugcs.com'
	)
}


def getChangeString() {
	MAX_MSG_LEN = 100
	def changeString = ""
	echo "Gathering SCM changes"
	def changeLogSets = currentBuild.rawBuild.changeSets
	for (int i = 0; i < changeLogSets.size(); i++) {
		def entries = changeLogSets[i].items
		for (int j = 0; j < entries.length; j++) {
			def entry = entries[j]
			changeString += "<p><b>${entry.msg}</b></p>\n<p>${entry.author}: <a href='https://bitbucket.org/sphengineering/ugcs-telemetry-viewer.git/commits/${entry.commitId}'>${entry.commitId}</a></p>\n<p></p>\n"
		}
	}
	if (!changeString) {
		changeString = " No new changes"
	}
	return changeString
} 