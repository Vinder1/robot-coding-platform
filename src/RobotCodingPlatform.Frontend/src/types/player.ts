export interface NullableUser {
    player: User;
    hasPlayer: boolean;
}

export interface User {
    id: string;
    name: string;
    email: string;
    role: string;
}
