// Maps the VSCode host (process.platform + process.arch) to a .NET runtime identifier.
export function getRuntimeIdentifier(): string {
    const isArm64 = process.arch === 'arm64';
    switch (process.platform) {
        case 'win32':
            return isArm64 ? 'win-arm64' : 'win-x64';
        case 'darwin':
            return isArm64 ? 'osx-arm64' : 'osx-x64';
        case 'linux':
            return isArm64 ? 'linux-arm64' : 'linux-x64';
        default:
            return 'linux-x64';
    }
}

export function getExecutableExtension(): string {
    return process.platform === 'win32' ? '.exe' : '';
}
